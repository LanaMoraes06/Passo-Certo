using Npgsql;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Models.Enum;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Repositories;

public class LojaRepository(NpgsqlDataSource dataSource) : ILojaRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    // SELECT - Listar lojas
    public async Task<List<Loja>> ListarAsync()
    {
        var lojas = new List<Loja>();

        const string sql = """
            SELECT id, codigo, nome, gerente_nome, regiao_id, ativo,
                   criado_em, atualizado_em
            FROM lojas
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
            lojas.Add(Mapear(reader));
        }

        return lojas;
    }

    // SELECT - Buscar loja por ID
    public async Task<Loja?> BuscarPorIdAsync(long id)
    {
        const string sql = """
            SELECT id, codigo, nome, gerente_nome, regiao_id, ativo,
                   criado_em, atualizado_em
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

    // SELECT - Buscar ativos
    public async Task<List<Loja>> ListarPorRegiaoAsync(long regiaoId)
   {
       var lojas = new List<Loja>();

       const string sql = """
           SELECT id, codigo, nome, gerente_nome, regiao_id, ativo,
                  criado_em, atualizado_em
           FROM lojas
           WHERE regiao_id = @RegiaoId
             AND ativo = TRUE
           ORDER BY nome
           """;

       await using var connection =
           await _dataSource.OpenConnectionAsync();

       await using var command =
           new NpgsqlCommand(sql, connection);

       command.Parameters.AddWithValue("RegiaoId", regiaoId);

       await using var reader =
           await command.ExecuteReaderAsync();

       while (await reader.ReadAsync())
       {
           lojas.Add(Mapear(reader));
       }

       return lojas;
   }
    // INSERT - Criar loja
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


    // UPDATE - Atualizar dados da loja
    public async Task<bool> AtualizarAsync(Loja loja)
    {
        const string sql = """
            UPDATE lojas
            SET
                codigo = @Codigo,
                nome = @Nome,
                gerente_nome = @GerenteNome,
                regiao_id = @RegiaoId,
                ativo = @Ativo,
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
        command.Parameters.AddWithValue("Ativo", loja.Ativo);

        int linhasAfetadas =
            await command.ExecuteNonQueryAsync();

        return linhasAfetadas > 0;
    }

    // UPDATE - Desativação lógica
    public async Task<bool> DesativarAsync(long id)
    {
        const string sql = """
            UPDATE lojas
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

    // Mapeamento: PostgreSQL -> Loja
    private static Loja Mapear(NpgsqlDataReader reader)
    {
        return new Loja
        {
            Id = reader.GetInt64(
                reader.GetOrdinal("id")),
            Codigo = reader.GetString(
                reader.GetOrdinal("codigo")),
            Nome = reader.GetString(
                reader.GetOrdinal("nome")),
            GerenteNome = reader.GetString(
                reader.GetOrdinal("gerente_nome")),
            RegiaoId = reader.GetInt64(
                reader.GetOrdinal("regiao_id")),
            Ativo = reader.GetBoolean(
                reader.GetOrdinal("ativo")),
            CriadoEm = reader.GetDateTime(
                reader.GetOrdinal("criado_em")),
            AtualizadoEm = reader.GetDateTime(
                reader.GetOrdinal("atualizado_em"))
        };
    }

}
