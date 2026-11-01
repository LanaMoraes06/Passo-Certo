using Npgsql;
using NpgsqlTypes;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Models.Enum;
using PassoCerto.Web.Repositories.Interfaces;
using PassoCerto.Web.Models.DTOs.Inspecoes;

namespace PassoCerto.Web.Repositories;

public class InspecaoRepository(NpgsqlDataSource dataSource)
    : IInspecaoRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    private const string FiltroAcesso = """
        EXISTS (
            SELECT 1
            FROM usuarios u
            WHERE u.id = @UsuarioId
              AND u.ativo = TRUE
              AND (
                  u.perfil IN ('DIRETORIA', 'ADMINISTRADOR')
                  OR (
                      u.perfil = 'SUPERVISOR'
                      AND EXISTS (
                          SELECT 1
                          FROM usuarios_regioes ur
                          WHERE ur.usuario_id = u.id
                            AND ur.regiao_id = i.regiao_id
                      )
                  )
              )
        )
        """;

    private const string ColunasInspecao = """
        i.id, i.loja_id, i.regiao_id,
        i.gerente_nome, i.autor_id, i.data_visita,
        i.observacao_geral, i.status,
        i.percentual_realizacao, i.concluida_em,
        i.criado_em, i.atualizado_em,
        i.criado_por, i.atualizado_por
        """;

    // SELECT - Listar inspeções autorizadas
    public async Task<List<Inspecao>> ListarAsync(long usuarioId)
    {
        var inspecoes = new List<Inspecao>();

        string sql = $"""
            SELECT {ColunasInspecao}
            FROM inspecoes i
            WHERE {FiltroAcesso}
            ORDER BY i.data_visita DESC, i.id DESC
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("UsuarioId", usuarioId);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            inspecoes.Add(Mapear(reader));

        return inspecoes;
    }

    public async Task<Inspecao?> BuscarPorIdAsync(
        long id, long usuarioId)
    {
        string sql = $"""
            SELECT {ColunasInspecao}
            FROM inspecoes i
            WHERE i.id = @Id
              AND {FiltroAcesso}
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", id);
        command.Parameters.AddWithValue("UsuarioId", usuarioId);

        await using var reader =
            await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? Mapear(reader)
            : null;
    }

    public async Task<List<Inspecao>> ListarPorLojaAsync(
        long lojaId, long usuarioId)
    {
        var inspecoes = new List<Inspecao>();

        string sql = $"""
            SELECT {ColunasInspecao}
            FROM inspecoes i
            WHERE i.loja_id = @LojaId
              AND {FiltroAcesso}
            ORDER BY i.data_visita DESC, i.id DESC
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("LojaId", lojaId);
        command.Parameters.AddWithValue("UsuarioId", usuarioId);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            inspecoes.Add(Mapear(reader));

        return inspecoes;
    }

    public async Task<List<RespostaInspecao>> ListarRespostasAsync(
        long inspecaoId, long usuarioId)
    {
        var respostas = new List<RespostaInspecao>();

        string sql = $"""
            SELECT r.*
            FROM respostas_inspecao r
            INNER JOIN inspecoes i ON i.id = r.inspecao_id
            WHERE i.id = @InspecaoId
              AND {FiltroAcesso}
            ORDER BY r.categoria_ordem, r.item_ordem
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "InspecaoId", inspecaoId);

        command.Parameters.AddWithValue(
            "UsuarioId", usuarioId);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            respostas.Add(MapearResposta(reader));

        return respostas;
    }

    public async Task<long> CriarAsync(Inspecao inspecao)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        const string sqlInspecao = """
            INSERT INTO inspecoes (
                loja_id,
                regiao_id,
                gerente_nome,
                autor_id,
                data_visita,
                observacao_geral,
                status,
                criado_por,
                atualizado_por
            )
            SELECT
                l.id,
                l.regiao_id,
                l.gerente_nome,
                u.id,
                @DataVisita,
                @ObservacaoGeral,
                'RASCUNHO',
                u.id,
                u.id
            FROM lojas l
            INNER JOIN usuarios u
                ON u.id = @AutorId
            WHERE l.id = @LojaId
              AND l.ativo = TRUE
              AND u.ativo = TRUE
              AND u.perfil = 'SUPERVISOR'
              AND @DataVisita <= CURRENT_DATE
              AND EXISTS (
                  SELECT 1
                  FROM usuarios_regioes ur
                  WHERE ur.usuario_id = u.id
                    AND ur.regiao_id = l.regiao_id
              )
            RETURNING id
            """;

        long inspecaoId;

        await using (var command =
            new NpgsqlCommand(sqlInspecao, connection, transaction))
        {
            command.Parameters.AddWithValue(
                "LojaId", inspecao.LojaId);

            command.Parameters.AddWithValue(
                "AutorId", inspecao.AutorId);

            command.Parameters.AddWithValue(
                "DataVisita", inspecao.DataVisita);

            command.Parameters.Add(
                "ObservacaoGeral", NpgsqlDbType.Text
            ).Value = (object?)inspecao.ObservacaoGeral
                       ?? DBNull.Value;

            var resultado = await command.ExecuteScalarAsync();

            if (resultado is null)
                throw new InvalidOperationException(
                    "Não foi possível criar a inspeção. " +
                    "Verifique a loja, a data e a abrangência.");

            inspecaoId = Convert.ToInt64(resultado);
        }


        const string sqlRespostas = """
            INSERT INTO respostas_inspecao (
                inspecao_id,
                item_id,
                categoria_nome,
                categoria_ordem,
                item_titulo,
                item_descricao,
                item_ordem,
                criado_por,
                atualizado_por
            )
            SELECT
                @InspecaoId,
                it.id,
                c.nome,
                c.ordem,
                it.titulo,
                it.descricao,
                it.ordem,
                @AutorId,
                @AutorId
            FROM itens_checklist it
            INNER JOIN categorias_checklist c
                ON c.id = it.categoria_id
            WHERE it.ativo = TRUE
              AND c.ativo = TRUE
            ORDER BY c.ordem, it.ordem
            """;

        await using (var command =
            new NpgsqlCommand(sqlRespostas, connection, transaction))
        {
            command.Parameters.AddWithValue(
                "InspecaoId", inspecaoId);

            command.Parameters.AddWithValue(
                "AutorId", inspecao.AutorId);

            int quantidade = await command.ExecuteNonQueryAsync();

            if (quantidade == 0)
                throw new InvalidOperationException(
                    "Não existem itens ativos no checklist.");
        }

        await transaction.CommitAsync();

        return inspecaoId;
    }

    public async Task<bool> AtualizarAsync(
        Inspecao inspecao, long usuarioId)
    {
        const string sql = """
            UPDATE inspecoes
            SET
                data_visita = @DataVisita,
                observacao_geral = @ObservacaoGeral,
                atualizado_em = NOW(),
                atualizado_por = @UsuarioId
            WHERE id = @Id
              AND autor_id = @UsuarioId
              AND status = 'RASCUNHO'
              AND @DataVisita <= CURRENT_DATE
              AND EXISTS (
                  SELECT 1
                  FROM usuarios u
                  WHERE u.id = @UsuarioId
                    AND u.ativo = TRUE
                    AND u.perfil = 'SUPERVISOR'
              )
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", inspecao.Id);
        command.Parameters.AddWithValue("UsuarioId", usuarioId);
        command.Parameters.AddWithValue(
            "DataVisita", inspecao.DataVisita);

        command.Parameters.Add(
            "ObservacaoGeral", NpgsqlDbType.Text
        ).Value = (object?)inspecao.ObservacaoGeral
                   ?? DBNull.Value;

        return await command.ExecuteNonQueryAsync() == 1;
    }

    public async Task<bool> SalvarRascunhoAsync(
        long inspecaoId,
        List<RespostaInspecao> respostas,
        long usuarioId)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        const string sqlVerificar = """
            SELECT i.id
            FROM inspecoes i
            INNER JOIN usuarios u ON u.id = i.autor_id
            WHERE i.id = @InspecaoId
              AND i.autor_id = @UsuarioId
              AND i.status = 'RASCUNHO'
              AND u.ativo = TRUE
              AND u.perfil = 'SUPERVISOR'
            FOR UPDATE OF i
            """;

        await using (var command =
            new NpgsqlCommand(sqlVerificar, connection, transaction))
        {
            command.Parameters.AddWithValue(
                "InspecaoId", inspecaoId);

            command.Parameters.AddWithValue(
                "UsuarioId", usuarioId);

            if (await command.ExecuteScalarAsync() is null)
                return false;
        }

        const string sqlResposta = """
            UPDATE respostas_inspecao
            SET
                resposta = @Resposta,
                observacao = @Observacao,
                atualizado_em = NOW(),
                atualizado_por = @UsuarioId
            WHERE id = @RespostaId
              AND inspecao_id = @InspecaoId
            """;

        if (respostas.Select(r => r.Id).Distinct().Count()
            != respostas.Count)
        {
            throw new InvalidOperationException(
                "Existem respostas duplicadas.");
        }

        foreach (var resposta in respostas)
        {
            await using var command =
                new NpgsqlCommand(sqlResposta, connection, transaction);

            command.Parameters.AddWithValue(
                "RespostaId", resposta.Id);

            command.Parameters.AddWithValue(
                "InspecaoId", inspecaoId);

            command.Parameters.AddWithValue(
                "UsuarioId", usuarioId);

            command.Parameters.Add(
                "Resposta", NpgsqlDbType.Varchar
            ).Value = resposta.Resposta.HasValue
                ? resposta.Resposta.Value.ToString()
                : DBNull.Value;

            command.Parameters.Add(
                "Observacao", NpgsqlDbType.Text
            ).Value = (object?)resposta.Observacao
                       ?? DBNull.Value;

            int linhas = await command.ExecuteNonQueryAsync();

            if (linhas != 1)
                throw new InvalidOperationException(
                    $"Resposta {resposta.Id} inválida para esta inspeção.");
        }

        const string sqlAuditoria = """
            UPDATE inspecoes
            SET
                atualizado_em = NOW(),
                atualizado_por = @UsuarioId
            WHERE id = @InspecaoId
            """;

        await using (var command =
            new NpgsqlCommand(sqlAuditoria, connection, transaction))
        {
            command.Parameters.AddWithValue(
                "InspecaoId", inspecaoId);

            command.Parameters.AddWithValue(
                "UsuarioId", usuarioId);

            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();

        return true;
    }

    // UPDATE - Concluir inspeção
    public async Task<bool> ConcluirAsync(
        long inspecaoId, long usuarioId)
    {
        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        const string sqlVerificar = """
            SELECT i.id
            FROM inspecoes i
            INNER JOIN usuarios u ON u.id = i.autor_id
            WHERE i.id = @InspecaoId
              AND i.autor_id = @UsuarioId
              AND i.status = 'RASCUNHO'
              AND u.ativo = TRUE
              AND u.perfil = 'SUPERVISOR'
            FOR UPDATE OF i
            """;

        await using (var command =
            new NpgsqlCommand(sqlVerificar, connection, transaction))
        {
            command.Parameters.AddWithValue(
                "InspecaoId", inspecaoId);

            command.Parameters.AddWithValue(
                "UsuarioId", usuarioId);

            if (await command.ExecuteScalarAsync() is null)
                return false;
        }

        const string sqlCalculo = """
            SELECT
                COUNT(*) AS total,
                COUNT(*) FILTER (
                    WHERE resposta IS NULL
                ) AS nao_respondidos,
                COUNT(*) FILTER (
                    WHERE resposta = 'PENDENTE'
                      AND NULLIF(BTRIM(observacao), '') IS NULL
                ) AS pendentes_invalidos,
                COUNT(*) FILTER (
                    WHERE resposta = 'OK'
                ) AS quantidade_ok,
                COUNT(*) FILTER (
                    WHERE resposta = 'PENDENTE'
                ) AS quantidade_pendente
            FROM respostas_inspecao
            WHERE inspecao_id = @InspecaoId
            """;

        long total;
        long naoRespondidos;
        long pendentesInvalidos;
        long quantidadeOk;
        long quantidadePendente;

        await using (var command =
            new NpgsqlCommand(sqlCalculo, connection, transaction))
        {
            command.Parameters.AddWithValue(
                "InspecaoId", inspecaoId);

            await using var reader =
                await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            total = reader.GetInt64(0);
            naoRespondidos = reader.GetInt64(1);
            pendentesInvalidos = reader.GetInt64(2);
            quantidadeOk = reader.GetInt64(3);
            quantidadePendente = reader.GetInt64(4);
        }

        if (total == 0 || naoRespondidos > 0 ||
            pendentesInvalidos > 0)
        {
            throw new InvalidOperationException(
                "A inspeção possui respostas incompletas ou inválidas.");
        }

        long aplicaveis = quantidadeOk + quantidadePendente;

        decimal? percentual = aplicaveis == 0
            ? null
            : Math.Round(
                (decimal)quantidadeOk / aplicaveis * 100m,
                1,
                MidpointRounding.AwayFromZero);

        const string sqlConcluir = """
            UPDATE inspecoes
            SET
                status = 'CONCLUIDA',
                percentual_realizacao = @Percentual,
                concluida_em = NOW(),
                atualizado_em = NOW(),
                atualizado_por = @UsuarioId
            WHERE id = @InspecaoId
              AND autor_id = @UsuarioId
              AND status = 'RASCUNHO'
            """;

        await using (var command =
            new NpgsqlCommand(sqlConcluir, connection, transaction))
        {
            command.Parameters.AddWithValue(
                "InspecaoId", inspecaoId);

            command.Parameters.AddWithValue(
                "UsuarioId", usuarioId);

            command.Parameters.Add(
                "Percentual", NpgsqlDbType.Numeric
            ).Value = (object?)percentual ?? DBNull.Value;

            if (await command.ExecuteNonQueryAsync() != 1)
                return false;
        }

        await transaction.CommitAsync();

        return true;
    }

    // MAPEAMENTO - Inspecao
    private static Inspecao Mapear(NpgsqlDataReader reader)
    {
        return new Inspecao
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            LojaId = reader.GetInt64(reader.GetOrdinal("loja_id")),
            RegiaoId = reader.GetInt64(reader.GetOrdinal("regiao_id")),
            GerenteNome = reader.GetString(
                reader.GetOrdinal("gerente_nome")),
            AutorId = reader.GetInt64(reader.GetOrdinal("autor_id")),
            DataVisita = reader.GetFieldValue<DateOnly>(
                reader.GetOrdinal("data_visita")),
            ObservacaoGeral = LerStringNullable(
                reader, "observacao_geral"),
            Status = Enum.Parse<StatusInspecao>(
                reader.GetString(reader.GetOrdinal("status")), ignoreCase: true),
            PercentualRealizacao = reader.IsDBNull(
                reader.GetOrdinal("percentual_realizacao"))
                ? null
                : reader.GetDecimal(
                    reader.GetOrdinal("percentual_realizacao")),
            ConcluidaEm = LerDataNullable(reader, "concluida_em"),
            CriadoEm = reader.GetDateTime(
                reader.GetOrdinal("criado_em")),
            AtualizadoEm = reader.GetDateTime(
                reader.GetOrdinal("atualizado_em")),
            CriadoPor = reader.GetInt64(
                reader.GetOrdinal("criado_por")),
            AtualizadoPor = reader.GetInt64(
                reader.GetOrdinal("atualizado_por"))
        };
    }


public async Task<(List<InspecaoResumoDto> Itens, long Total)>
    BuscarHistoricoAsync(FiltroHistorico filtro, long usuarioId)
{
    var itens = new List<InspecaoResumoDto>();

    string where = $"""
        WHERE {FiltroAcesso}
          AND (@DataInicio IS NULL OR i.data_visita >= @DataInicio)
          AND (@DataFim IS NULL OR i.data_visita <= @DataFim)
          AND (@LojaId IS NULL OR i.loja_id = @LojaId)
          AND (@RegiaoId IS NULL OR i.regiao_id = @RegiaoId)
          AND (@Status IS NULL OR i.status = @Status)
        """;

    string sqlTotal = $"""
        SELECT COUNT(*)
        FROM inspecoes i
        {where}
        """;

    string sqlItens = $"""
        SELECT
            i.id,
            l.nome AS loja_nome,
            rg.nome AS regiao_nome,
            i.data_visita,
            i.status,
            i.percentual_realizacao
        FROM inspecoes i
        INNER JOIN lojas l ON l.id = i.loja_id
        INNER JOIN regioes rg ON rg.id = i.regiao_id
        {where}
        ORDER BY i.data_visita DESC, i.id DESC
        LIMIT @Limite OFFSET @Deslocamento
        """;

    await using var connection =
        await _dataSource.OpenConnectionAsync();

    void AdicionarFiltros(NpgsqlCommand command)
    {
        command.Parameters.AddWithValue("UsuarioId", usuarioId);

        command.Parameters.Add("DataInicio", NpgsqlDbType.Date).Value =
            (object?)filtro.DataInicio ?? DBNull.Value;

        command.Parameters.Add("DataFim", NpgsqlDbType.Date).Value =
            (object?)filtro.DataFim ?? DBNull.Value;

        command.Parameters.Add("LojaId", NpgsqlDbType.Bigint).Value =
            (object?)filtro.LojaId ?? DBNull.Value;

        command.Parameters.Add("RegiaoId", NpgsqlDbType.Bigint).Value =
            (object?)filtro.RegiaoId ?? DBNull.Value;

        command.Parameters.Add("Status", NpgsqlDbType.Varchar).Value =
            filtro.Status.HasValue
                ? filtro.Status.Value.ToString()
                : DBNull.Value;
    }

    long total;

    await using (var command = new NpgsqlCommand(sqlTotal, connection))
    {
        AdicionarFiltros(command);

        total = Convert.ToInt64(
            await command.ExecuteScalarAsync());
    }

    await using (var command = new NpgsqlCommand(sqlItens, connection))
    {
        AdicionarFiltros(command);

        command.Parameters.AddWithValue(
            "Limite", filtro.TamanhoPagina);

        command.Parameters.AddWithValue(
            "Deslocamento",
            (filtro.Pagina - 1) * filtro.TamanhoPagina);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            itens.Add(new InspecaoResumoDto
            {
                Id = reader.GetInt64(
                    reader.GetOrdinal("id")),

                LojaNome = reader.GetString(
                    reader.GetOrdinal("loja_nome")),

                RegiaoNome = reader.GetString(
                    reader.GetOrdinal("regiao_nome")),

                DataVisita = reader.GetFieldValue<DateOnly>(
                    reader.GetOrdinal("data_visita")),

                Status = Enum.Parse<StatusInspecao>(
                    reader.GetString(reader.GetOrdinal("status")),
                    ignoreCase: true),

                PercentualRealizacao =
                    reader.IsDBNull(
                        reader.GetOrdinal("percentual_realizacao"))
                    ? null
                    : reader.GetDecimal(
                        reader.GetOrdinal("percentual_realizacao"))
            });
        }
    }

    return (itens, total);
}


    private static RespostaInspecao MapearResposta(
        NpgsqlDataReader reader)
    {
        var respostaTexto =
            LerStringNullable(reader, "resposta");

        return new RespostaInspecao
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            InspecaoId = reader.GetInt64(
                reader.GetOrdinal("inspecao_id")),
            ItemId = reader.GetInt64(reader.GetOrdinal("item_id")),
            CategoriaNome = reader.GetString(
                reader.GetOrdinal("categoria_nome")),
            CategoriaOrdem = reader.GetInt32(
                reader.GetOrdinal("categoria_ordem")),
            ItemTitulo = reader.GetString(
                reader.GetOrdinal("item_titulo")),
            ItemDescricao = reader.GetString(
                reader.GetOrdinal("item_descricao")),
            ItemOrdem = reader.GetInt32(
                reader.GetOrdinal("item_ordem")),
            Resposta = respostaTexto is null
                ? null
                : Enum.Parse<TipoResposta>(respostaTexto, ignoreCase: true),
            Observacao = LerStringNullable(reader, "observacao"),
            CriadoEm = reader.GetDateTime(
                reader.GetOrdinal("criado_em")),
            AtualizadoEm = reader.GetDateTime(
                reader.GetOrdinal("atualizado_em")),
            CriadoPor = reader.GetInt64(
                reader.GetOrdinal("criado_por")),
            AtualizadoPor = reader.GetInt64(
                reader.GetOrdinal("atualizado_por"))
        };
    }

    private static string? LerStringNullable(
        NpgsqlDataReader reader, string coluna)
    {
        int indice = reader.GetOrdinal(coluna);

        return reader.IsDBNull(indice)
            ? null
            : reader.GetString(indice);
    }

    private static DateTime? LerDataNullable(
        NpgsqlDataReader reader, string coluna)
    {
        int indice = reader.GetOrdinal(coluna);

        return reader.IsDBNull(indice)
            ? null
            : reader.GetDateTime(indice);
    }
}
