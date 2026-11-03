namespace PassoCerto.Web.Repositories.Interfaces;

public interface IUsuarioRegiaoRepository
{
    Task<List<long>> ListarRegioesAsync(long usuarioId);

    Task SubstituirRegioesAsync(
        long usuarioId,
        List<long> regioesIds);
}