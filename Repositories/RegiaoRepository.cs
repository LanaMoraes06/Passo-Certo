using Npgsql;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Models.Enum;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Repositories;

public class RegiaoRepository(NpgsqlDataSource dataSource) : IRegiaoRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    // SELECT - Listar regioes
    public async Task<List<Regiao>> ListarAsync()
    {
        var regioes = new List<Regiao>();

        const string sql = """
            SELECT id, nome, ativo,
                   criado_em, atualizado_em
            FROM regioes
            ORDER BY nome
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);
                  
                
        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            regioes.Add(Mapear(reader));
        }

        return regioes;
    }

    // SELECT - Buscar região por ID
    public async Task<Regiao?> BuscarPorIdAsync(long id)
    {
        const string sql = """
            SELECT id, nome, ativo,
                   criado_em, atualizado_em
            FROM regioes
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

    // SELECT - Buscar ativos
   public async Task<List<Regiao>> ListarAtivasAsync()
{
    var regioes = new List<Regiao>();

    const string sql = """
        SELECT id, nome, ativo,
               criado_em, atualizado_em
        FROM regioes
        WHERE ativo = TRUE
        ORDER BY nome
        """;

    await using var connection =
        await _dataSource.OpenConnectionAsync();

    await using var command =
        new NpgsqlCommand(sql, connection);

    await using var reader =
        await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        regioes.Add(Mapear(reader));
    }

    return regioes;
}

    // INSERT - Criar região
    public async Task<long> CriarAsync(Regiao regiao)
    {
        const string sql = """
            INSERT INTO regioes
                (nome, ativo)
            VALUES
                (@Nome, @Ativo)
            RETURNING id
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Nome", regiao.Nome);
        command.Parameters.AddWithValue("Ativo", regiao.Ativo);

        var resultado = await command.ExecuteScalarAsync();

        return Convert.ToInt64(resultado);
    }


    // UPDATE - Atualizar dados da região
    public async Task<bool> AtualizarAsync(Regiao regiao)
    {
        const string sql = """
            UPDATE regioes
            SET
                nome = @Nome,
                ativo = @Ativo,
                atualizado_em = NOW()
            WHERE id = @Id
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", regiao.Id);
        command.Parameters.AddWithValue("Nome", regiao.Nome);
        command.Parameters.AddWithValue("Ativo", regiao.Ativo);

        int linhasAfetadas =
            await command.ExecuteNonQueryAsync();

        return linhasAfetadas > 0;
    }


    // UPDATE - Desativação lógica
    public async Task<bool> DesativarAsync(long id)
    {
        const string sql = """
            UPDATE regioes
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

    // Mapeamento: PostgreSQL -> Regiao
    private static Regiao Mapear(NpgsqlDataReader reader)
    {
        return new Regiao
        {
            Id = reader.GetInt64(
                reader.GetOrdinal("id")),
            Nome = reader.GetString(
                reader.GetOrdinal("nome")),
            Ativo = reader.GetBoolean(
                reader.GetOrdinal("ativo")),
            CriadoEm = reader.GetDateTime(
                reader.GetOrdinal("criado_em")),
            AtualizadoEm = reader.GetDateTime(
                reader.GetOrdinal("atualizado_em"))
        };
    }

}
