using Npgsql;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Models.Enum;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UsuarioRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // SELECT - Listar usuários
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

    // SELECT - Buscar usuário por ID
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

    // SELECT - Buscar usuário pelo login
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

    // SELECT - Regiões vinculadas ao supervisor
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

    // INSERT - Criar usuário
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

    // UPDATE - Atualizar dados do usuário
    public async Task<bool> AtualizarAsync(Usuario usuario)
    {
        const string sql = """
            UPDATE usuarios
            SET
                login = @Login,
                perfil = @Perfil,
                ativo = @Ativo,
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
        command.Parameters.AddWithValue("Ativo", usuario.Ativo);

        int linhasAfetadas =
            await command.ExecuteNonQueryAsync();

        return linhasAfetadas > 0;
    }

    // UPDATE - Alterar somente a senha
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

    // UPDATE - Desativação lógica
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

    // Mapeamento: PostgreSQL -> Usuario
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
                    reader.GetOrdinal("perfil"))),

            Ativo = reader.GetBoolean(
                reader.GetOrdinal("ativo")),

            CriadoEm = reader.GetDateTime(
                reader.GetOrdinal("criado_em")),

            AtualizadoEm = reader.GetDateTime(
                reader.GetOrdinal("atualizado_em"))
        };
    }

    Task<List<Usuario>> IUsuarioRepository.ListarRegioesPorUsuarioAsync(long usuarioId)
    {
        throw new NotImplementedException();
    }
}
