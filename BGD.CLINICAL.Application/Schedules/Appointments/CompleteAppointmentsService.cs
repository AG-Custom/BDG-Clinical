using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.PatientApplications;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
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
    private readonly IPatientApplicationsRepository _patientApplicationsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteAppointmentsService(
        ICurrentTenantContext tenantContext,
        IAppointmentsRepository appointmentsRepository,
        IProceduresRepository proceduresRepository,
        IProductsRepository productsRepository,
        IStockBalancesRepository stockBalancesRepository,
        IStockMovementsRepository stockMovementsRepository,
        IPatientApplicationsRepository patientApplicationsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _appointmentsRepository = appointmentsRepository;
        _proceduresRepository = proceduresRepository;
        _productsRepository = productsRepository;
        _stockBalancesRepository = stockBalancesRepository;
        _stockMovementsRepository = stockMovementsRepository;
        _patientApplicationsRepository = patientApplicationsRepository;
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

        if (agendamento.AplicacaoPaciente is not null)
        {
            return Result<AppointmentDto>.Failure("Este agendamento já possui aplicação vinculada.");
        }

        var dadosAnteriores = AppointmentsAuditSerializer.Serialize(agendamento);

        try
        {
            if (agendamento.Tipo == TipoAgendamento.Aplicacao)
            {
                var applicationError = await CreateApplicationFromAppointmentAsync(
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

    private async Task<string?> CreateApplicationFromAppointmentAsync(
        Agendamento agendamento,
        CompleteAppointmentRequest request,
        Guid empresaId,
        CancellationToken cancellationToken)
    {
        if (!agendamento.ProcedimentoId.HasValue)
        {
            return "Agendamento de aplicação sem procedimento vinculado.";
        }

        var procedimento = await _proceduresRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            agendamento.ProcedimentoId.Value,
            empresaId,
            cancellationToken);

        if (procedimento is null || !procedimento.Ativo)
        {
            return "Procedimento não encontrado ou inativo.";
        }

        if (procedimento.ProdutoAplicadoId.HasValue)
        {
            if (!request.QuantidadeUtilizada.HasValue || request.QuantidadeUtilizada.Value <= 0)
            {
                return "A quantidade utilizada deve ser maior que zero.";
            }
        }
        else if (request.QuantidadeUtilizada.HasValue)
        {
            return "Quantidade utilizada não se aplica a procedimentos sem produto aplicado.";
        }

        if (request.Peso.HasValue && request.Peso.Value <= 0)
        {
            return "O peso deve ser maior que zero quando informado.";
        }

        var productIds = new HashSet<Guid>();
        if (procedimento.ProdutoAplicadoId.HasValue)
        {
            productIds.Add(procedimento.ProdutoAplicadoId.Value);
        }

        foreach (var item in procedimento.Itens)
        {
            productIds.Add(item.ProdutoId);
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
        var stockLines = PatientApplicationStockPlanner.BuildLines(
            request.QuantidadeUtilizada,
            procedimento,
            productsById);

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
            procedimento.ProdutoAplicadoId,
            procedimento.Id,
            agendamento.FuncionarioId,
            agendamento.UnidadeId,
            agendamento.DataInicio,
            request.QuantidadeUtilizada,
            request.Peso,
            agendamento.Observacao,
            agendamento.Id);

        var movimentacoes = stockLines
            .Where(line => line.ControlaEstoque)
            .Select(line => MovimentacaoEstoque.CreateSaidaFromAplicacao(
                empresaId,
                agendamento.UnidadeId,
                line.ProdutoId,
                aplicacao.Id,
                agendamento.FuncionarioId,
                line.Quantidade,
                agendamento.DataInicio))
            .ToList();

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
