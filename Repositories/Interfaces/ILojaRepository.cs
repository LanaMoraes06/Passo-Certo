using PassoCerto.Web.Models.Entity;

namespace PassoCerto.Web.Repositories.Interfaces;

public interface ILojaRepository : IRepository<Loja>
{
    Task<List<Loja>> ListarPorRegiaoAsync(long regiaoId);

    Task<List<Loja>> ListarPermitidasAsync(long usuarioId);

    Task<Loja?> BuscarParaInspecaoAsync(
        long lojaId,
        long usuarioId);

    Task<bool> ExisteNomeAsync(
        string nome,
        long? ignorarId = null);

    Task<bool> ExisteCodigoAsync(
        string codigo,
        long? ignorarId = null);

    Task<bool> ReativarAsync(long id);
}
