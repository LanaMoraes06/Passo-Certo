using Npgsql;
using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Repositories;

public class UsuarioRegiaoRepository(
    NpgsqlDataSource dataSource) : IUsuarioRegiaoRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    public async Task<List<long>> ListarRegioesAsync(long usuarioId)
    {
        var regioesIds = new List<long>();

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
            regioesIds.Add(reader.GetInt64(0));
        }

        return regioesIds;
    }

    public async Task SubstituirRegioesAsync(
        long usuarioId,
        List<long> regioesIds)
    {
        ArgumentNullException.ThrowIfNull(regioesIds);

        const string lockSql = """
            SELECT id
            FROM usuarios
            WHERE id = @UsuarioId
            FOR UPDATE
            """;

        const string deleteSql = """
            DELETE FROM usuarios_regioes
            WHERE usuario_id = @UsuarioId
            """;

        const string insertSql = """
            INSERT INTO usuarios_regioes (usuario_id, regiao_id)
            VALUES (@UsuarioId, @RegiaoId)
            """;

        await using var connection =
            await _dataSource.OpenConnectionAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        try
        {
            // 1. Bloqueia a linha do usuário até finalizar
            // a transação.
            await using (var lockCommand =
                new NpgsqlCommand(
                    lockSql,
                    connection,
                    transaction))
            {
                lockCommand.Parameters.AddWithValue(
                    "UsuarioId", usuarioId);

                var resultado =
                    await lockCommand.ExecuteScalarAsync();

                if (resultado is null)
                    throw new NotFoundException(
                        "Usuário não encontrado.");
            }

            // 2. Remove os vínculos regionais anteriores.
            await using (var deleteCommand =
                new NpgsqlCommand(
                    deleteSql,
                    connection,
                    transaction))
            {
                deleteCommand.Parameters.AddWithValue(
                    "UsuarioId", usuarioId);

                await deleteCommand.ExecuteNonQueryAsync();
            }

            // 3. Insere as regiões selecionadas.
            foreach (var regiaoId in regioesIds.Distinct())
            {
                await using var insertCommand =
                    new NpgsqlCommand(
                        insertSql,
                        connection,
                        transaction);

                insertCommand.Parameters.AddWithValue(
                    "UsuarioId", usuarioId);

                insertCommand.Parameters.AddWithValue(
                    "RegiaoId", regiaoId);

                await insertCommand.ExecuteNonQueryAsync();
            }

            // 4. Confirma todas as alterações.
            await transaction.CommitAsync();
        }
        catch
        {
            // 5. Desfaz tudo caso alguma operação falhe.
            await transaction.RollbackAsync();
            throw;
        }
    }
}
