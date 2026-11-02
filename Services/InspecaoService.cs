using PassoCerto.Web.Exceptions;
using PassoCerto.Web.Models.DTOs.Inspecoes;
using PassoCerto.Web.Models.Entity;
using PassoCerto.Web.Models.Enum;
using PassoCerto.Web.Repositories.Interfaces;

namespace PassoCerto.Web.Services;

public class InspecaoService(
    IInspecaoRepository inspecaoRepository,
    ILojaRepository lojaRepository)
{
    public async Task<long> CriarAsync(
        CriarInspecaoInput input,
        long usuarioId)
    {
        ArgumentNullException.ThrowIfNull(input);

        ValidarUsuario(usuarioId);

        if (input.LojaId is null || input.LojaId <= 0)
            throw new BusinessException("Selecione uma loja válida.");

        if (input.DataVisita is null)
            throw new BusinessException("Informe a data da visita.");

        ValidarData(input.DataVisita.Value);

        var loja = await lojaRepository.BuscarParaInspecaoAsync(
            input.LojaId.Value, usuarioId);

        if (loja is null)
            throw new NotFoundException(
                "Loja não encontrada ou indisponível para inspeção.");

        var inspecao = new Inspecao
        {
            LojaId = loja.Id,
            AutorId = usuarioId,
            DataVisita = input.DataVisita.Value,
            ObservacaoGeral = Normalizar(input.ObservacaoGeral)
        };

        return await inspecaoRepository.CriarAsync(inspecao);
    }

    // 2. LISTAR INSPEÇÕES
    public Task<List<Inspecao>> ListarAsync(long usuarioId)
    {
        ValidarUsuario(usuarioId);

        return inspecaoRepository.ListarAsync(usuarioId);
    }

    public async Task<Inspecao> BuscarPorIdAsync(
        long inspecaoId,
        long usuarioId)
    {
        ValidarIds(inspecaoId, usuarioId);

        return await inspecaoRepository.BuscarPorIdAsync(
            inspecaoId, usuarioId)
            ?? throw new NotFoundException(
                "Inspeção não encontrada.");
    }

    public Task<List<Inspecao>> ListarPorLojaAsync(
        long lojaId,
        long usuarioId)
    {
        ValidarIds(lojaId, usuarioId);

        return inspecaoRepository.ListarPorLojaAsync(
            lojaId, usuarioId);
    }

    
    public Task<List<RespostaInspecao>> ListarRespostasAsync(
        long inspecaoId,
        long usuarioId)
    {
        ValidarIds(inspecaoId, usuarioId);

        return inspecaoRepository.ListarRespostasAsync(
            inspecaoId, usuarioId);
    }

    public async Task AtualizarAsync(
        long inspecaoId,
        EditarInspecaoInput input,
        long usuarioId)
    {
        ArgumentNullException.ThrowIfNull(input);
        ValidarIds(inspecaoId, usuarioId);
        ValidarData(input.DataVisita);

        var inspecao = await BuscarPorIdAsync(
            inspecaoId, usuarioId);

        ValidarRascunhoDoAutor(inspecao, usuarioId);

        inspecao.DataVisita = input.DataVisita;
        inspecao.ObservacaoGeral = Normalizar(
            input.ObservacaoGeral);

        bool atualizado = await inspecaoRepository
            .AtualizarAsync(inspecao, usuarioId);

        if (!atualizado)
            throw new BusinessException(
                "Não foi possível atualizar o rascunho.");
    }

    public async Task SalvarRascunhoAsync(
        long inspecaoId,
        SalvarRascunhoInput input,
        long usuarioId)
    {
        ArgumentNullException.ThrowIfNull(input);
        ValidarIds(inspecaoId, usuarioId);

        var inspecao = await BuscarPorIdAsync(
            inspecaoId, usuarioId);

        ValidarRascunhoDoAutor(inspecao, usuarioId);

        if (input.Respostas is null)
            throw new BusinessException("Respostas inválidas.");

        if (input.Respostas.Any(r => r.Id <= 0))
            throw new BusinessException(
                "Há identificadores de respostas inválidos.");

        if (input.Respostas.Select(r => r.Id).Distinct().Count()
            != input.Respostas.Count)
            throw new BusinessException(
                "Existem respostas duplicadas.");

        var respostas = new List<RespostaInspecao>();

        foreach (var item in input.Respostas)
        {
            string? observacao = Normalizar(item.Observacao);

            if (item.Resposta is not null &&
                !Enum.IsDefined(typeof(TipoResposta), item.Resposta.Value))
            {
                throw new BusinessException(
                    "Foi informada uma resposta inválida.");
            }

            if (EhPendente(item.Resposta) &&
                observacao is null)
            {
                throw new BusinessException(
                    "Uma resposta pendente exige observação.");
            }

            respostas.Add(new RespostaInspecao
            {
                Id = item.Id,
                Resposta = item.Resposta,
                Observacao = observacao
            });
        }

        bool salvo = await inspecaoRepository
            .SalvarRascunhoAsync(inspecaoId, respostas, usuarioId);

        if (!salvo)
            throw new BusinessException(
                "Não foi possível salvar o rascunho.");
    }

    public async Task ConcluirAsync(
        long inspecaoId,
        long usuarioId)
    {
        ValidarIds(inspecaoId, usuarioId);

        var inspecao = await BuscarPorIdAsync(
            inspecaoId, usuarioId);

        ValidarRascunhoDoAutor(inspecao, usuarioId);

        var respostas = await ListarRespostasAsync(
            inspecaoId, usuarioId);

        var erros = new List<string>();

        if (respostas.Count == 0)
            erros.Add("A inspeção não possui itens.");

        foreach (var item in respostas)
        {
            if (item.Resposta is null)
            {
                erros.Add(
                    $"O item '{item.ItemTitulo}' não foi respondido.");
            }
            else if (EhPendente(item.Resposta) &&
                     string.IsNullOrWhiteSpace(item.Observacao))
            {
                erros.Add(
                    $"O item '{item.ItemTitulo}' exige observação.");
            }
        }

        if (erros.Count > 0)
            throw new BusinessException(
                string.Join(Environment.NewLine, erros));

      
        bool concluida = await inspecaoRepository
            .ConcluirAsync(inspecaoId, usuarioId);

        if (!concluida)
            throw new BusinessException(
                "Não foi possível concluir a inspeção.");
    }


    private static void ValidarUsuario(long usuarioId)
    {
        if (usuarioId <= 0)
            throw new BusinessException("Usuário inválido.");
    }

    private static void ValidarIds(long id, long usuarioId)
    {
        if (id <= 0)
            throw new BusinessException(
                "Identificador inválido.");

        ValidarUsuario(usuarioId);
    }

    private static string? Normalizar(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto)
            ? null
            : texto.Trim();
    }

    private static bool EhPendente(TipoResposta? resposta)
    {
        return resposta?.ToString().Equals(
            "PENDENTE",
            StringComparison.OrdinalIgnoreCase) == true;
    }

    private static void ValidarData(DateOnly data)
    {
        var fuso = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Sao_Paulo");

        var agoraBrasil = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow, fuso);

        var hoje = DateOnly.FromDateTime(agoraBrasil);

        if (data == default)
            throw new BusinessException(
                "Informe uma data válida.");

        if (data > hoje)
            throw new BusinessException(
                "A data da visita não pode ser futura.");
    }
    public async Task<(List<InspecaoResumoDto> Itens, long Total)>
    BuscarHistoricoAsync(FiltroHistorico filtro, long usuarioId)
{
    ArgumentNullException.ThrowIfNull(filtro);
    ValidarUsuario(usuarioId);

    if (filtro.DataInicio.HasValue &&
        filtro.DataFim.HasValue &&
        filtro.DataInicio > filtro.DataFim)
    {
        throw new BusinessException(
            "A data inicial não pode ser maior que a data final.");
    }

    if (filtro.LojaId is <= 0 || filtro.RegiaoId is <= 0)
    {
        throw new BusinessException(
            "Loja ou região inválida.");
    }

    if (filtro.Status.HasValue &&
        !Enum.IsDefined(filtro.Status.Value))
    {
        throw new BusinessException(
            "Status inválido.");
    }

    filtro.Pagina = Math.Max(1, filtro.Pagina);
    filtro.TamanhoPagina = 10;

    return await inspecaoRepository
        .BuscarHistoricoAsync(filtro, usuarioId);
}

    private static void ValidarRascunhoDoAutor(
        Inspecao inspecao,
        long usuarioId)
    {
        if (inspecao.AutorId != usuarioId)
            throw new ForbiddenException(
                "Você não pode alterar esta inspeção.");

        if (!inspecao.Status.ToString().Equals(
            "RASCUNHO",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(
                "Uma inspeção concluída não pode ser alterada.");
        }
    }

    
}
