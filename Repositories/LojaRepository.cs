using Npgsql;
using NpgsqlTypes;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Repositories;

public class LojaRepository(NpgsqlDataSource dataSource) : ILojaRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    public async Task<List<Loja>> ListarAsync()
    {
        const string sql = """
            SELECT id, codigo, nome, gerente_nome, regiao_id,
                   ativo, criado_em, atualizado_em
            FROM lojas
            ORDER BY nome
            """;

        var lojas = new List<Loja>();

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            lojas.Add(Mapear(reader));

        return lojas;
    }

    public async Task<Loja?> BuscarPorIdAsync(long id)
    {
        const string sql = """
            SELECT id, codigo, nome, gerente_nome, regiao_id,
                   ativo, criado_em, atualizado_em
            FROM lojas
            WHERE id = @Id
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", id);

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return Mapear(reader);
    }

    public async Task<List<Loja>> ListarPorRegiaoAsync(long regiaoId)
    {
        const string sql = """
            SELECT id, codigo, nome, gerente_nome, regiao_id,
                   ativo, criado_em, atualizado_em
            FROM lojas
            WHERE regiao_id = @RegiaoId
              AND ativo = TRUE
            ORDER BY nome
            """;

        var lojas = new List<Loja>();

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("RegiaoId", regiaoId);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            lojas.Add(Mapear(reader));

        return lojas;
    }

    public async Task<long> CriarAsync(Loja loja)
    {
        const string sql = """
            INSERT INTO lojas
                (codigo, nome, gerente_nome, regiao_id, ativo)
            VALUES
                (@Codigo, @Nome, @GerenteNome, @RegiaoId, @Ativo)
            RETURNING id
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Codigo", loja.Codigo);
        command.Parameters.AddWithValue("Nome", loja.Nome);
        command.Parameters.AddWithValue("GerenteNome", loja.GerenteNome);
        command.Parameters.AddWithValue("RegiaoId", loja.RegiaoId);
        command.Parameters.AddWithValue("Ativo", loja.Ativo);

        var resultado = await command.ExecuteScalarAsync();

        return Convert.ToInt64(resultado);
    }

    public async Task<bool> AtualizarAsync(Loja loja)
    {
        const string sql = """
            UPDATE lojas
            SET codigo = @Codigo,
                nome = @Nome,
                gerente_nome = @GerenteNome,
                regiao_id = @RegiaoId,
                atualizado_em = NOW()
            WHERE id = @Id
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", loja.Id);
        command.Parameters.AddWithValue("Codigo", loja.Codigo);
        command.Parameters.AddWithValue("Nome", loja.Nome);
        command.Parameters.AddWithValue("GerenteNome", loja.GerenteNome);
        command.Parameters.AddWithValue("RegiaoId", loja.RegiaoId);

        int linhasAfetadas =
            await command.ExecuteNonQueryAsync();

        return linhasAfetadas > 0;
    }

    public async Task<bool> DesativarAsync(long id)
    {
        const string sql = """
            UPDATE lojas
            SET ativo = FALSE,
                atualizado_em = NOW()
            WHERE id = @Id
              AND ativo = TRUE
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", id);

        int linhasAfetadas =
            await command.ExecuteNonQueryAsync();

        return linhasAfetadas > 0;
    }

    public async Task<bool> ReativarAsync(long id)
    {
        const string sql = """
            UPDATE lojas
            SET ativo = TRUE,
                atualizado_em = NOW()
            WHERE id = @Id
              AND ativo = FALSE
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", id);

        int linhasAfetadas =
            await command.ExecuteNonQueryAsync();

        return linhasAfetadas > 0;
    }

    public async Task<List<Loja>> ListarPermitidasAsync(long usuarioId)
    {
        const string sql = """
            SELECT l.id, l.codigo, l.nome, l.gerente_nome,
                   l.regiao_id, l.ativo, l.criado_em,
                   l.atualizado_em
            FROM lojas l
            INNER JOIN usuarios u ON u.id = @UsuarioId
            WHERE u.ativo = TRUE
              AND l.ativo = TRUE
              AND (
                  u.perfil IN ('DIRETORIA', 'ADMINISTRADOR')
                  OR (
                      u.perfil = 'SUPERVISOR'
                      AND EXISTS (
                          SELECT 1
                          FROM usuarios_regioes ur
                          WHERE ur.usuario_id = u.id
                            AND ur.regiao_id = l.regiao_id
                      )
                  )
              )
            ORDER BY l.nome
            """;

        var lojas = new List<Loja>();

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("UsuarioId", usuarioId);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            lojas.Add(Mapear(reader));

        return lojas;
    }

    public async Task<Loja?> BuscarParaInspecaoAsync(
        long lojaId,
        long usuarioId)
    {
        const string sql = """
            SELECT l.id, l.codigo, l.nome, l.gerente_nome,
                   l.regiao_id, l.ativo, l.criado_em,
                   l.atualizado_em
            FROM lojas l
            INNER JOIN usuarios u ON u.id = @UsuarioId
            WHERE l.id = @LojaId
              AND l.ativo = TRUE
              AND u.ativo = TRUE
              AND u.perfil = 'SUPERVISOR'
              AND EXISTS (
                  SELECT 1
                  FROM usuarios_regioes ur
                  WHERE ur.usuario_id = u.id
                    AND ur.regiao_id = l.regiao_id
              )
            LIMIT 1
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("LojaId", lojaId);
        command.Parameters.AddWithValue("UsuarioId", usuarioId);

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return Mapear(reader);
    }

    public async Task<bool> ExisteNomeAsync(
        string nome,
        long? ignorarId = null)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM lojas
                WHERE LOWER(TRIM(nome)) = LOWER(TRIM(@Nome))
                  AND (@IgnorarId IS NULL OR id <> @IgnorarId)
            )
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Nome", nome);
        command.Parameters.Add("IgnorarId", NpgsqlDbType.Bigint)
            .Value = (object?)ignorarId ?? DBNull.Value;

        return (bool)(await command.ExecuteScalarAsync() ?? false);
    }

    public async Task<bool> ExisteCodigoAsync(
        string codigo,
        long? ignorarId = null)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM lojas
                WHERE LOWER(TRIM(codigo)) = LOWER(TRIM(@Codigo))
                  AND (@IgnorarId IS NULL OR id <> @IgnorarId)
            )
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Codigo", codigo);
        command.Parameters.Add("IgnorarId", NpgsqlDbType.Bigint)
            .Value = (object?)ignorarId ?? DBNull.Value;

        return (bool)(await command.ExecuteScalarAsync() ?? false);
    }

    private static Loja Mapear(NpgsqlDataReader reader)
    {
        int atualizadoOrdinal =
            reader.GetOrdinal("atualizado_em");

        return new Loja
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            Codigo = reader.GetString(reader.GetOrdinal("codigo")),
            Nome = reader.GetString(reader.GetOrdinal("nome")),
            GerenteNome = reader.GetString(
                reader.GetOrdinal("gerente_nome")),
            RegiaoId = reader.GetInt64(
                reader.GetOrdinal("regiao_id")),
            Ativo = reader.GetBoolean(reader.GetOrdinal("ativo")),
            CriadoEm = reader.GetDateTime(
                reader.GetOrdinal("criado_em")),

            AtualizadoEm = reader.IsDBNull(atualizadoOrdinal)
                ? default
                : reader.GetFieldValue<DateTimeOffset>(
                    atualizadoOrdinal).DateTime
        };
    }
}
