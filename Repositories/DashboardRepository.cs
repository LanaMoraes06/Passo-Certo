using Npgsql;
using NpgsqlTypes;
using PassoCerto.Web.Models.DTOs.Dashboard;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Repositories;

public class DashboardRepository(NpgsqlDataSource dataSource)
    : IDashboardRepository
{
    private const string LojasVisiveis = """
        SELECT l.id, l.codigo, l.nome, l.regiao_id
        FROM lojas l
        JOIN usuarios u ON u.id = @UsuarioId
        WHERE u.ativo = TRUE
          AND l.ativo = TRUE
          AND (@RegiaoId IS NULL OR l.regiao_id = @RegiaoId)
          AND (@LojaId IS NULL OR l.id = @LojaId)
          AND (
              u.perfil IN ('DIRETORIA', 'ADMINISTRADOR')
              OR (
                  u.perfil = 'SUPERVISOR'
                  AND EXISTS (
                      SELECT 1 FROM usuarios_regioes ur
                      WHERE ur.usuario_id = u.id
                        AND ur.regiao_id = l.regiao_id
                  )
              )
          )
        """;

    private const string InspecoesVisiveis = """
        SELECT i.id, i.loja_id, i.regiao_id, i.data_visita,
               i.status, i.percentual_realizacao
        FROM inspecoes i
        JOIN usuarios u ON u.id = @UsuarioId
        WHERE u.ativo = TRUE
          AND (@DataInicio IS NULL OR i.data_visita >= @DataInicio)
          AND (@DataFim IS NULL OR i.data_visita <= @DataFim)
          AND (@RegiaoId IS NULL OR i.regiao_id = @RegiaoId)
          AND (@LojaId IS NULL OR i.loja_id = @LojaId)
          AND (
              u.perfil IN ('DIRETORIA', 'ADMINISTRADOR')
              OR (
                  u.perfil = 'SUPERVISOR'
                  AND EXISTS (
                      SELECT 1 FROM usuarios_regioes ur
                      WHERE ur.usuario_id = u.id
                        AND ur.regiao_id = i.regiao_id
                  )
              )
          )
        """;

    private static void AdicionarParametros(
        NpgsqlCommand comando,
        FiltroDashboard filtro,
        long usuarioId)
    {
        comando.Parameters.AddWithValue("UsuarioId", usuarioId);

        comando.Parameters.Add("RegiaoId", NpgsqlDbType.Bigint)
            .Value = (object?)filtro.RegiaoId ?? DBNull.Value;

        comando.Parameters.Add("LojaId", NpgsqlDbType.Bigint)
            .Value = (object?)filtro.LojaId ?? DBNull.Value;

        comando.Parameters.Add("DataInicio", NpgsqlDbType.Date)
            .Value = (object?)filtro.DataInicio ?? DBNull.Value;

        comando.Parameters.Add("DataFim", NpgsqlDbType.Date)
            .Value = (object?)filtro.DataFim ?? DBNull.Value;
    }

    // RESUMO GERAL
    public async Task<ResumoDashboard> ObterResumoAsync(
        FiltroDashboard filtro, long usuarioId)
    {
        string sql = $"""
            WITH lojas_visiveis AS (
                {LojasVisiveis}
            ),
            inspecoes_visiveis AS (
                {InspecoesVisiveis}
            ),
            concluidas AS (
                SELECT * FROM inspecoes_visiveis
                WHERE status = 'CONCLUIDA'
            ),
            contagens AS (
                SELECT
                    COUNT(*) AS total,
                    COUNT(*) FILTER (WHERE status = 'CONCLUIDA')
                        AS concluidas,
                    COUNT(*) FILTER (WHERE status = 'RASCUNHO')
                        AS rascunhos
                FROM inspecoes_visiveis
            ),
            indicadores AS (
                SELECT
                    AVG(c.percentual_realizacao) AS percentual_medio,
                    COUNT(DISTINCT c.loja_id) AS lojas_inspecionadas
                FROM concluidas c
                JOIN lojas_visiveis l ON l.id = c.loja_id
                    AND l.regiao_id = c.regiao_id
            ),
            respostas AS (
                SELECT
                    COUNT(*) FILTER (WHERE r.resposta = 'OK') AS ok,
                    COUNT(*) FILTER (WHERE r.resposta = 'PENDENTE') AS pendentes,
                    COUNT(*) FILTER (WHERE r.resposta = 'NA') AS na
                FROM respostas_inspecao r
                JOIN concluidas c ON c.id = r.inspecao_id
            )
            SELECT
                ct.total, ct.concluidas, ct.rascunhos,
                ind.percentual_medio,
                (SELECT COUNT(*) FROM lojas_visiveis),
                ind.lojas_inspecionadas,
                resp.ok, resp.pendentes, resp.na
            FROM contagens ct
            CROSS JOIN indicadores ind
            CROSS JOIN respostas resp
            """;

        await using var conexao =
            await dataSource.OpenConnectionAsync();

        await using var comando = new NpgsqlCommand(sql, conexao);
        AdicionarParametros(comando, filtro, usuarioId);

        await using var reader = await comando.ExecuteReaderAsync();
        await reader.ReadAsync();

        return new ResumoDashboard
        {
            TotalInspecoes = reader.GetInt64(0),
            Concluidas = reader.GetInt64(1),
            Rascunhos = reader.GetInt64(2),
            PercentualMedio = reader.IsDBNull(3)
                ? null : reader.GetDecimal(3),
            TotalLojasAtivas = reader.GetInt64(4),
            LojasInspecionadas = reader.GetInt64(5),
            QuantidadeOk = reader.GetInt64(6),
            QuantidadePendentes = reader.GetInt64(7),
            QuantidadeNa = reader.GetInt64(8)
        };
    }

    public async Task<List<LojaNaoInspecionada>>
        ObterLojasNaoInspecionadasAsync(
            FiltroDashboard filtro, long usuarioId)
    {
        string sql = $"""
            WITH lojas_visiveis AS (
                {LojasVisiveis}
            ),
            inspecoes_visiveis AS (
                {InspecoesVisiveis}
            )
            SELECT l.id, l.codigo, l.nome, rg.nome,
                   (
                       SELECT MAX(i.data_visita)
                       FROM inspecoes i
                       WHERE i.loja_id = l.id
                         AND i.status = 'CONCLUIDA'
                   ) AS ultima_visita
            FROM lojas_visiveis l
            JOIN regioes rg ON rg.id = l.regiao_id
            WHERE NOT EXISTS (
                SELECT 1 FROM inspecoes_visiveis i
                WHERE i.loja_id = l.id
                  AND i.status = 'CONCLUIDA'
            )
            ORDER BY rg.nome, l.nome
            """;

        var resultado = new List<LojaNaoInspecionada>();

        await using var conexao =
            await dataSource.OpenConnectionAsync();

        await using var comando = new NpgsqlCommand(sql, conexao);
        AdicionarParametros(comando, filtro, usuarioId);

        await using var reader = await comando.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            resultado.Add(new LojaNaoInspecionada
            {
                LojaId = reader.GetInt64(0),
                Codigo = reader.GetString(1),
                Nome = reader.GetString(2),
                Regiao = reader.GetString(3),
                UltimaVisita = reader.IsDBNull(4)
                    ? null
                    : reader.GetFieldValue<DateOnly>(4)
            });
        }

        return resultado;
    }

    public async Task<List<ItemPendenteResumo>> ObterTopPendenciasAsync(
        FiltroDashboard filtro, long usuarioId)
    {
        string sql = $"""
            WITH inspecoes_visiveis AS (
                {InspecoesVisiveis}
            )
            SELECT
                r.item_id,
                MAX(r.item_titulo),
                COUNT(*) AS quantidade,
                COUNT(DISTINCT i.loja_id) AS lojas_afetadas
            FROM respostas_inspecao r
            JOIN inspecoes_visiveis i ON i.id = r.inspecao_id
            WHERE i.status = 'CONCLUIDA'
              AND r.resposta = 'PENDENTE'
            GROUP BY r.item_id
            ORDER BY quantidade DESC, r.item_id
            LIMIT 5
            """;

        var resultado = new List<ItemPendenteResumo>();

        await using var conexao =
            await dataSource.OpenConnectionAsync();

        await using var comando = new NpgsqlCommand(sql, conexao);
        AdicionarParametros(comando, filtro, usuarioId);

        await using var reader = await comando.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            resultado.Add(new ItemPendenteResumo
            {
                ItemId = reader.GetInt64(0),
                Titulo = reader.GetString(1),
                QuantidadePendencias = reader.GetInt64(2),
                LojasAfetadas = reader.GetInt64(3)
            });
        }

        return resultado;
    }

    public async Task<List<ResumoRegiao>> ObterResumoPorRegiaoAsync(
        FiltroDashboard filtro, long usuarioId)
    {
        string sql = $"""
            WITH lojas_visiveis AS (
                {LojasVisiveis}
            ),
            inspecoes_visiveis AS (
                {InspecoesVisiveis}
            ),
            concluidas AS (
                SELECT * FROM inspecoes_visiveis
                WHERE status = 'CONCLUIDA'
            ),
            lojas_regiao AS (
                SELECT regiao_id, COUNT(*) AS total_lojas
                FROM lojas_visiveis
                GROUP BY regiao_id
            ),
            indicadores AS (
                SELECT
                    c.regiao_id,
                    COUNT(*) AS total_inspecoes,
                    COUNT(DISTINCT c.loja_id) AS lojas_inspecionadas,
                    AVG(c.percentual_realizacao) AS percentual_medio
                FROM concluidas c
                JOIN lojas_visiveis l ON l.id = c.loja_id
                    AND l.regiao_id = c.regiao_id
                GROUP BY c.regiao_id
            ),
            pendencias AS (
                SELECT
                    c.regiao_id,
                    COUNT(*) AS total
                FROM concluidas c
                JOIN respostas_inspecao r ON r.inspecao_id = c.id
                WHERE r.resposta = 'PENDENTE'
                GROUP BY c.regiao_id
            )
            SELECT
                rg.id, rg.nome, lr.total_lojas,
                COALESCE(ind.lojas_inspecionadas, 0),
                COALESCE(ind.total_inspecoes, 0),
                ind.percentual_medio,
                COALESCE(p.total, 0)
            FROM lojas_regiao lr
            JOIN regioes rg ON rg.id = lr.regiao_id
            LEFT JOIN indicadores ind ON ind.regiao_id = rg.id
            LEFT JOIN pendencias p ON p.regiao_id = rg.id
            ORDER BY rg.nome
            """;

        var resultado = new List<ResumoRegiao>();

        await using var conexao =
            await dataSource.OpenConnectionAsync();

        await using var comando = new NpgsqlCommand(sql, conexao);
        AdicionarParametros(comando, filtro, usuarioId);

        await using var reader = await comando.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            resultado.Add(new ResumoRegiao
            {
                RegiaoId = reader.GetInt64(0),
                Nome = reader.GetString(1),
                TotalLojas = reader.GetInt64(2),
                LojasInspecionadas = reader.GetInt64(3),
                TotalInspecoes = reader.GetInt64(4),
                PercentualMedio = reader.IsDBNull(5)
                    ? null : reader.GetDecimal(5),
                Pendencias = reader.GetInt64(6)
            });
        }

        return resultado;
    }
}
