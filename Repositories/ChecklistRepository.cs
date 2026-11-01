using Npgsql;
using NpgsqlTypes;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Repositories;

public class ChecklistRepository(NpgsqlDataSource dataSource) : IChecklistRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    public async Task<List<ItemChecklist>> ListarItensAtivosAsync()
    {
        const string sql = """
            SELECT id, categoria_id, nome, descricao, ordem, ativo,
                   criado_em, atualizado_em
            FROM itens_checklist
            WHERE ativo = TRUE
            ORDER BY categoria_id, ordem
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var itens = new List<ItemChecklist>();
        while (await reader.ReadAsync())
        {
            itens.Add(MapearItem(reader));
        }

        return itens;
    }

    public async Task<List<ItemChecklist>> ListarPorCategoriaAsync(long categoriaId)
    {
        const string sql = """
            SELECT id, categoria_id, nome, descricao, ordem, ativo,
                   criado_em, atualizado_em
            FROM itens_checklist
            WHERE categoria_id = @CategoriaId
            ORDER BY ordem
            """;

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("CategoriaId", categoriaId);
        await using var reader = await command.ExecuteReaderAsync();

        var itens = new List<ItemChecklist>();
        while (await reader.ReadAsync())
        {
            itens.Add(MapearItem(reader));
        }

        return itens;
    }

    public async Task<List<CategoriaChecklist>> ListarCategoriasAsync()
    {
        var categorias = new List<CategoriaChecklist>();

        const string sql = """
            SELECT id, nome, ordem, ativo,
                   criado_em, atualizado_em
            FROM categorias_checklist
            ORDER BY ordem
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categorias.Add(Mapear(reader));
        }

        return categorias;
    }

    public async Task<CategoriaChecklist?> BuscarPorIdAsync(long id)
    {
        const string sql = """
            SELECT id, nome, ordem, ativo,
                   criado_em, atualizado_em
            FROM categorias_checklist
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

    public async Task<long> CriarAsync(
        CategoriaChecklist categoria)
    {
        const string sql = """
            INSERT INTO categorias_checklist
                (nome, ordem, ativo)
            VALUES
                (@Nome, @Ordem, @Ativo)
            RETURNING id
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "Nome", categoria.Nome);

        command.Parameters.AddWithValue(
            "Ordem", categoria.Ordem);

        command.Parameters.AddWithValue(
            "Ativo", categoria.Ativo);

        var resultado = await command.ExecuteScalarAsync();

        return Convert.ToInt64(resultado);
    }

    public async Task<bool> AtualizarAsync(
        CategoriaChecklist categoria)
    {
        const string sql = """
            UPDATE categorias_checklist
            SET
                nome = @Nome,
                ordem = @Ordem,
                ativo = @Ativo,
                atualizado_em = NOW()
            WHERE id = @Id
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("Id", categoria.Id);
        command.Parameters.AddWithValue("Nome", categoria.Nome);
        command.Parameters.AddWithValue("Ordem", categoria.Ordem);
        command.Parameters.AddWithValue("Ativo", categoria.Ativo);

        int linhasAfetadas =
            await command.ExecuteNonQueryAsync();

        return linhasAfetadas > 0;
    }

    public async Task<bool> DesativarAsync(long id)
    {
        const string sql = """
            UPDATE categorias_checklist
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

    private static CategoriaChecklist Mapear(
        NpgsqlDataReader reader)
    {
        return new CategoriaChecklist
        {
            Id = reader.GetInt64(
                reader.GetOrdinal("id")),

            Nome = reader.GetString(
                reader.GetOrdinal("nome")),

            Ordem = reader.GetInt32(
                reader.GetOrdinal("ordem")),

            Ativo = reader.GetBoolean(
                reader.GetOrdinal("ativo")),

            CriadoEm = reader.GetDateTime(
                reader.GetOrdinal("criado_em")),

            AtualizadoEm = reader.GetDateTime(
                reader.GetOrdinal("atualizado_em"))
        };
    }

    private static ItemChecklist MapearItem(NpgsqlDataReader reader)
    {
        return new ItemChecklist
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            CategoriaId = reader.GetInt64(reader.GetOrdinal("categoria_id")),
            Titulo = reader.GetString(reader.GetOrdinal("titulo")),
            Descricao = reader.IsDBNull(reader.GetOrdinal("descricao"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("descricao")),
            Ordem = reader.GetInt32(reader.GetOrdinal("ordem")),
            Ativo = reader.GetBoolean(reader.GetOrdinal("ativo")),
            CriadoEm = reader.GetDateTime(reader.GetOrdinal("criado_em")),
            AtualizadoEm = reader.GetDateTime(reader.GetOrdinal("atualizado_em"))
        };
    }

    public Task<List<CategoriaChecklist>> ListarCategoriasAsync(long categoriaId)
    {
        throw new NotImplementedException();
    }
}
