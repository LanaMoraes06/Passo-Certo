using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.Enum;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Services;

public class UsuarioRegiaoService(
    IUsuarioRegiaoRepository usuarioRegiaoRepository,
    UsuarioService usuarioService,
    RegiaoService regiaoService)
{
    public async Task<List<long>> ListarRegioesAsync(long usuarioId)
    {
        await ValidarSupervisorAsync(usuarioId);

        return await usuarioRegiaoRepository
            .ListarRegioesAsync(usuarioId);
    }

    public async Task SubstituirRegioesAsync(
        long usuarioId,
        List<long> regioesIds)
    {
        if (regioesIds is null)
            throw new BusinessException(
                "A lista de regiões é obrigatória.");

        await ValidarSupervisorAsync(usuarioId);

        var idsDistintos = regioesIds
            .Distinct()
            .ToList();

        foreach (var regiaoId in idsDistintos)
        {
            if (regiaoId <= 0)
                throw new BusinessException(
                    "Identificador de região inválido.");

            var regiao = await regiaoService
                .BuscarPorIdAsync(regiaoId);

            if (!regiao.Ativo)
                throw new BusinessException(
                    $"A região '{regiao.Nome}' está inativa.");
        }

        await usuarioRegiaoRepository
            .SubstituirRegioesAsync(usuarioId, idsDistintos);
    }

    private async Task ValidarSupervisorAsync(long usuarioId)
    {
        if (usuarioId <= 0)
            throw new BusinessException(
                "Identificador de usuário inválido.");

        var usuario = await usuarioService.BuscarPorIdAsync(usuarioId);

        if (!usuario.Ativo)
            throw new BusinessException(
                "Não é possível gerenciar regiões de um usuário inativo.");

        if (usuario.Perfil != PerfilUsuario.SUPERVISOR)
            throw new BusinessException(
                "Somente supervisores podem possuir vínculos regionais.");
    }
}
