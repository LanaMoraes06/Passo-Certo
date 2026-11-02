using Npgsql;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Models.Enum;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Repositories;

public class UsuarioRepository(NpgsqlDataSource dataSource) : IUsuarioRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    public async Task<List<Usuario>> ListarAsync()
    {
        var usuarios = new List<Usuario>();

        const string sql = """
            SELECT id, login, senha_hash, perfil,
                   ativo, criado_em, atualizado_em
            FROM usuarios
            ORDER BY login
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            usuarios.Add(Mapear(reader));
        }

        return usuarios;
    }

    public async Task<Usuario?> BuscarPorIdAsync(long id)
    {
        const string sql = """
            SELECT id, login, senha_hash, perfil,
                   ativo, criado_em, atualizado_em
            FROM usuarios
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

    public async Task<Usuario?> BuscarPorLoginAsync(string login)
    {
        const string sql = """
            SELECT id, login, senha_hash, perfil,
                   ativo, criado_em, atualizado_em
            FROM usuarios
            WHERE login = @Login
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Login", login);

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return Mapear(reader);
    }

    public async Task<List<long>> ListarRegioesPorUsuarioAsync(
        long usuarioId)
    {
        var regioes = new List<long>();

        const string sql = """
            SELECT regiao_id
            FROM usuarios_regioes
            WHERE usuario_id = @UsuarioId
            ORDER BY regiao_id
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("UsuarioId", usuarioId);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            regioes.Add(reader.GetInt64(0));
        }

        return regioes;
    }

    public async Task<long> CriarAsync(Usuario usuario)
    {
        const string sql = """
            INSERT INTO usuarios
                (login, senha_hash, perfil, ativo)
            VALUES
                (@Login, @SenhaHash, @Perfil, @Ativo)
            RETURNING id
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Login", usuario.Login);
        command.Parameters.AddWithValue("SenhaHash", usuario.SenhaHash);
        command.Parameters.AddWithValue(
            "Perfil", usuario.Perfil.ToString());
        command.Parameters.AddWithValue("Ativo", usuario.Ativo);

        var resultado = await command.ExecuteScalarAsync();

        return Convert.ToInt64(resultado);
    }

    public async Task<bool> AtualizarAsync(Usuario usuario)
    {
        const string sql = """
            UPDATE usuarios
            SET
                login = @Login,
                perfil = @Perfil,
                atualizado_em = NOW()
            WHERE id = @Id
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", usuario.Id);
        command.Parameters.AddWithValue("Login", usuario.Login);
        command.Parameters.AddWithValue(
            "Perfil", usuario.Perfil.ToString());

        int linhasAfetadas =
            await command.ExecuteNonQueryAsync();

        return linhasAfetadas > 0;
    }

    public async Task<bool> AtualizarSenhaHashAsync(
        long id,
        string novoHash)
    {
        const string sql = """
            UPDATE usuarios
            SET
                senha_hash = @NovoHash,
                atualizado_em = NOW()
            WHERE id = @Id
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", id);
        command.Parameters.AddWithValue("NovoHash", novoHash);

        int linhasAfetadas =
            await command.ExecuteNonQueryAsync();

        return linhasAfetadas > 0;
    }

    public async Task<bool> DesativarAsync(long id)
    {
        const string sql = """
            UPDATE usuarios
            SET
                ativo = FALSE,
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

    private static Usuario Mapear(NpgsqlDataReader reader)
    {
        return new Usuario
        {
            Id = reader.GetInt64(
                reader.GetOrdinal("id")),

            Login = reader.GetString(
                reader.GetOrdinal("login")),

            SenhaHash = reader.GetString(
                reader.GetOrdinal("senha_hash")),

            Perfil = Enum.Parse<PerfilUsuario>(
                reader.GetString(
                    reader.GetOrdinal("perfil")),
                     ignoreCase: true),

            Ativo = reader.GetBoolean(
                reader.GetOrdinal("ativo")),

            CriadoEm = reader.GetDateTime(
                reader.GetOrdinal("criado_em")),

            AtualizadoEm = reader.GetDateTime(
                reader.GetOrdinal("atualizado_em"))
            };
        }

       public async Task<bool> ExisteLoginAsync(
        string login,
        long? ignorarId = null)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM usuarios
                WHERE LOWER(TRIM(login)) = LOWER(TRIM(@Login))
                  AND (@IgnorarId IS NULL OR id <> @IgnorarId)
            )
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Login", login);

        command.Parameters.Add(
            "IgnorarId",
            NpgsqlTypes.NpgsqlDbType.Bigint
        ).Value = (object?)ignorarId ?? DBNull.Value;

        return (bool)(await command.ExecuteScalarAsync() ?? false);
    }

    public async Task<bool> ReativarAsync(long id)
    {
        const string sql = """
            UPDATE usuarios
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

        return await command.ExecuteNonQueryAsync() > 0;
    }
    }
