using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Applications.PatientApplications;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.StockMovements;
using BGD.CLINICAL.Application.Packages.Abstractions;
using BGD.CLINICAL.Application.Schedules.Abstractions;
using BGD.CLINICAL.Application.Schedules.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Schedules.Appointments;

public interface ICompleteAppointmentsService
{
    Task<Result<AppointmentDto>> ExecuteAsync(
        Guid id,
        CompleteAppointmentRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CompleteAppointmentsService : ICompleteAppointmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAppointmentsRepository _appointmentsRepository;
    private readonly IProceduresRepository _proceduresRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IStockBalancesRepository _stockBalancesRepository;
    private readonly IStockMovementsRepository _stockMovementsRepository;
    private readonly IMedicationLotStockService _medicationLotStockService;
    private readonly IPatientApplicationsRepository _patientApplicationsRepository;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteAppointmentsService(
        ICurrentTenantContext tenantContext,
        IAppointmentsRepository appointmentsRepository,
        IProceduresRepository proceduresRepository,
        IProductsRepository productsRepository,
        IStockBalancesRepository stockBalancesRepository,
        IStockMovementsRepository stockMovementsRepository,
        IMedicationLotStockService medicationLotStockService,
        IPatientApplicationsRepository patientApplicationsRepository,
        IPatientPurchasesRepository patientPurchasesRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _appointmentsRepository = appointmentsRepository;
        _proceduresRepository = proceduresRepository;
        _productsRepository = productsRepository;
        _stockBalancesRepository = stockBalancesRepository;
        _stockMovementsRepository = stockMovementsRepository;
        _medicationLotStockService = medicationLotStockService;
        _patientApplicationsRepository = patientApplicationsRepository;
        _patientPurchasesRepository = patientPurchasesRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AppointmentDto>> ExecuteAsync(
        Guid id,
        CompleteAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var agendamento = await _appointmentsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            empresaId,
            cancellationToken);

        if (agendamento is null)
        {
            return Result<AppointmentDto>.Failure("Agendamento não encontrado.");
        }

        if (agendamento.AplicacoesPaciente.Any(aplicacao => !aplicacao.Cancelada))
        {
            return Result<AppointmentDto>.Failure("Este agendamento já possui aplicação vinculada.");
        }

        var dadosAnteriores = AppointmentsAuditSerializer.Serialize(agendamento);

        try
        {
            if (agendamento.Tipo == TipoAgendamento.Aplicacao)
            {
                var applicationError = await CreateApplicationsFromAppointmentAsync(
                    agendamento,
                    request,
                    empresaId,
                    cancellationToken);

                if (applicationError is not null)
                {
                    return Result<AppointmentDto>.Failure(applicationError);
                }
            }

            agendamento.MarkAsCompleted();
            _appointmentsRepository.Update(agendamento);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _appointmentsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Agendamento),
                id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: AppointmentsAuditSerializer.Serialize(persisted ?? agendamento),
                cancellationToken: cancellationToken);

            return Result<AppointmentDto>.Success(AppointmentsMapper.Map(persisted ?? agendamento));
        }
        catch (DomainException exception)
        {
            return Result<AppointmentDto>.Failure(exception.Message);
        }
    }

    private async Task<string?> CreateApplicationsFromAppointmentAsync(
        Agendamento agendamento,
        CompleteAppointmentRequest request,
        Guid empresaId,
        CancellationToken cancellationToken)
    {
        if (agendamento.Tipo == TipoAgendamento.Aplicacao
            && (!agendamento.CompraPacienteId.HasValue || agendamento.CompraPacienteId == Guid.Empty))
        {
            return "Informe a compra de pacote no agendamento antes de concluir.";
        }

        var procedimentoIds = agendamento.GetProcedimentoIds();
        if (procedimentoIds.Count == 0)
        {
            return "Agendamento de aplicação sem procedimento vinculado.";
        }

        var completeItems = ResolveCompleteItems(procedimentoIds, request);
        if (completeItems.IsFailure)
        {
            return completeItems.Error;
        }

        foreach (var item in completeItems.Value!)
        {
            var error = await CreateApplicationForProcedureAsync(
                agendamento,
                item.ProcedimentoId,
                item.QuantidadeUtilizada,
                item.Peso,
                item.LoteProdutoId,
                item.ConsumirInsumosKit,
                item.InsumosManuais,
                empresaId,
                cancellationToken);

            if (error is not null)
            {
                return error;
            }
        }

        if (agendamento.CompraPacienteId.HasValue)
        {
            var compra = await _patientPurchasesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                agendamento.CompraPacienteId.Value,
                empresaId,
                cancellationToken);

            if (compra is not null)
            {
                compra.CompleteIfExhausted();
                _patientPurchasesRepository.Update(compra);
            }
        }

        return null;
    }

    private static Result<IReadOnlyList<CompleteAppointmentProcedureRequest>> ResolveCompleteItems(
        IReadOnlyList<Guid> procedimentoIds,
        CompleteAppointmentRequest request)
    {
        if (procedimentoIds.Count == 1)
        {
            return Result<IReadOnlyList<CompleteAppointmentProcedureRequest>>.Success(
            [
                new CompleteAppointmentProcedureRequest(
                    procedimentoIds[0],
                    request.QuantidadeUtilizada,
                    request.Peso,
                    request.LoteProdutoId,
                    request.ConsumirInsumosKit,
                    request.InsumosManuais)
            ]);
        }

        if (request.Procedimentos is null || request.Procedimentos.Count == 0)
        {
            return Result<IReadOnlyList<CompleteAppointmentProcedureRequest>>.Failure(
                "Informe os dados de conclusão de cada procedimento em procedimentos.");
        }

        if (request.Procedimentos.Count != procedimentoIds.Count)
        {
            return Result<IReadOnlyList<CompleteAppointmentProcedureRequest>>.Failure(
                "Informe os dados de conclusão para todos os procedimentos do agendamento.");
        }

        var expectedIds = procedimentoIds.ToHashSet();
        var informedIds = request.Procedimentos.Select(item => item.ProcedimentoId).ToList();

        if (informedIds.Any(id => id == Guid.Empty) || informedIds.Distinct().Count() != informedIds.Count)
        {
            return Result<IReadOnlyList<CompleteAppointmentProcedureRequest>>.Failure(
                "Informe procedimentos distintos e válidos na conclusão.");
        }

        if (!expectedIds.SetEquals(informedIds))
        {
            return Result<IReadOnlyList<CompleteAppointmentProcedureRequest>>.Failure(
                "Os procedimentos informados na conclusão devem corresponder aos do agendamento.");
        }

        return Result<IReadOnlyList<CompleteAppointmentProcedureRequest>>.Success(request.Procedimentos);
    }

    private async Task<string?> CreateApplicationForProcedureAsync(
        Agendamento agendamento,
        Guid procedimentoId,
        decimal? quantidadeUtilizada,
        decimal? peso,
        Guid? loteProdutoId,
        bool consumirInsumosKit,
        IReadOnlyList<PatientApplicationManualSupplyRequest>? insumosManuais,
        Guid empresaId,
        CancellationToken cancellationToken)
    {
        var procedimento = await _proceduresRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            procedimentoId,
            empresaId,
            cancellationToken);

        if (procedimento is null || !procedimento.Ativo)
        {
            return "Procedimento não encontrado ou inativo.";
        }

        if (procedimento.ProdutoAplicadoId.HasValue)
        {
            if (!quantidadeUtilizada.HasValue || quantidadeUtilizada.Value <= 0)
            {
                return "A quantidade utilizada deve ser maior que zero.";
            }
        }
        else if (quantidadeUtilizada.HasValue)
        {
            return "Quantidade utilizada não se aplica a procedimentos sem produto aplicado.";
        }

        if (peso.HasValue && peso.Value <= 0)
        {
            return "O peso deve ser maior que zero quando informado.";
        }

        var productIds = new HashSet<Guid>();
        if (procedimento.ProdutoAplicadoId.HasValue)
        {
            productIds.Add(procedimento.ProdutoAplicadoId.Value);
        }

        if (consumirInsumosKit)
        {
            foreach (var item in procedimento.Itens)
            {
                productIds.Add(item.ProdutoId);
            }
        }
        else
        {
            foreach (var manual in insumosManuais ?? [])
            {
                if (manual.ProdutoId != Guid.Empty)
                {
                    productIds.Add(manual.ProdutoId);
                }
            }
        }

        var produtos = await _productsRepository.GetActiveByIdsAndEmpresaIdAsync(
            empresaId,
            productIds,
            cancellationToken);

        if (produtos.Count != productIds.Count)
        {
            return "Um ou mais produtos do consumo não foram encontrados ou estão inativos.";
        }

        var productsById = produtos.ToDictionary(produto => produto.Id);

        var insumosManuaisValidados = PatientApplicationManualSuppliesValidator.NormalizeAndValidate(
            consumirInsumosKit,
            insumosManuais,
            procedimento.ProdutoAplicadoId,
            productsById);

        if (insumosManuaisValidados.IsFailure)
        {
            return insumosManuaisValidados.Error;
        }

        var stockLines = PatientApplicationStockPlanner.BuildLines(
            quantidadeUtilizada,
            procedimento,
            productsById,
            consumirInsumosKit,
            insumosManuaisValidados.Value);

        Produto? produtoAplicado = null;
        if (procedimento.ProdutoAplicadoId.HasValue
            && productsById.TryGetValue(procedimento.ProdutoAplicadoId.Value, out var produtoResolvido))
        {
            produtoAplicado = produtoResolvido;
        }

        // Lote opcional temporariamente (pós-migração). Quando informado, valida alocação;
        // sem lote, segue baixa de estoque sem vínculo de lote.
        if (produtoAplicado is not null && _medicationLotStockService.RequiresLot(produtoAplicado))
        {
            if (loteProdutoId.HasValue && loteProdutoId.Value != Guid.Empty)
            {
                try
                {
                    await _medicationLotStockService.AllocateFromLotAsync(
                        empresaId,
                        agendamento.UnidadeId,
                        produtoAplicado,
                        loteProdutoId.Value,
                        quantidadeUtilizada!.Value,
                        cancellationToken);
                }
                catch (DomainException exception)
                {
                    return exception.Message;
                }
            }
            else
            {
                loteProdutoId = null;
            }
        }
        else if (loteProdutoId.HasValue && loteProdutoId.Value != Guid.Empty)
        {
            return "Lote só pode ser informado para procedimentos com medicamento que controla lote.";
        }
        else
        {
            loteProdutoId = null;
        }

        foreach (var line in stockLines.Where(line => line.ControlaEstoque))
        {
            var saldo = await _stockBalancesRepository.GetSaldoByUnidadeAndProdutoAsync(
                empresaId,
                agendamento.UnidadeId,
                line.ProdutoId,
                cancellationToken);

            if (saldo < line.Quantidade)
            {
                return $"Estoque insuficiente para \"{line.ProdutoNome}\" na unidade selecionada. Saldo: {saldo} | Necessário: {line.Quantidade}";
            }
        }

        var aplicacao = AplicacaoPaciente.CreateRealizada(
            empresaId,
            agendamento.PacienteId,
            agendamento.CompraPacienteId,
            procedimento.ProdutoAplicadoId,
            procedimento.Id,
            agendamento.FuncionarioId,
            agendamento.UnidadeId,
            agendamento.DataInicio,
            quantidadeUtilizada,
            peso,
            agendamento.Observacao,
            agendamento.Id);

        var movimentacoes = new List<MovimentacaoEstoque>();

        foreach (var line in stockLines.Where(line => line.ControlaEstoque))
        {
            var isProdutoAplicado = procedimento.ProdutoAplicadoId.HasValue
                && line.ProdutoId == procedimento.ProdutoAplicadoId.Value;

            if (isProdutoAplicado
                && produtoAplicado is not null
                && _medicationLotStockService.RequiresLot(produtoAplicado)
                && loteProdutoId.HasValue)
            {
                var alocacao = await _medicationLotStockService.AllocateFromLotAsync(
                    empresaId,
                    agendamento.UnidadeId,
                    produtoAplicado,
                    loteProdutoId.Value,
                    line.Quantidade,
                    cancellationToken);

                var movimentacao = MovimentacaoEstoque.CreateSaidaFromAplicacao(
                    empresaId,
                    agendamento.UnidadeId,
                    line.ProdutoId,
                    aplicacao.Id,
                    agendamento.FuncionarioId,
                    alocacao.Quantidade,
                    agendamento.DataInicio);
                movimentacao.AssignLote(alocacao.LoteProdutoId);
                movimentacoes.Add(movimentacao);
            }
            else
            {
                movimentacoes.Add(MovimentacaoEstoque.CreateSaidaFromAplicacao(
                    empresaId,
                    agendamento.UnidadeId,
                    line.ProdutoId,
                    aplicacao.Id,
                    agendamento.FuncionarioId,
                    line.Quantidade,
                    agendamento.DataInicio));
            }
        }

        await _patientApplicationsRepository.AddAsync(aplicacao, cancellationToken);

        if (movimentacoes.Count > 0)
        {
            await _stockMovementsRepository.AddRangeAsync(movimentacoes, cancellationToken);
        }

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(AplicacaoPaciente),
            aplicacao.Id,
            AcaoAuditoria.Criar,
            dadosNovos: PatientApplicationsAuditSerializer.Serialize(aplicacao),
            cancellationToken: cancellationToken);

        foreach (var movimentacao in movimentacoes)
        {
            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(MovimentacaoEstoque),
                movimentacao.Id,
                AcaoAuditoria.GerarMovimentacao,
                dadosNovos: PatientApplicationsAuditSerializer.Serialize(movimentacao),
                cancellationToken: cancellationToken);
        }

        return null;
    }
}
