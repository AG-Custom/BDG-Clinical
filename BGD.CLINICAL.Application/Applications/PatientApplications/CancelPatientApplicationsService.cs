using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Packages.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

public interface ICancelPatientApplicationsService
{
    Task<Result<PatientApplicationDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class CancelPatientApplicationsService : ICancelPatientApplicationsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientApplicationsRepository _patientApplicationsRepository;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;
    private readonly IStockMovementsRepository _stockMovementsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CancelPatientApplicationsService(
        ICurrentTenantContext tenantContext,
        IPatientApplicationsRepository patientApplicationsRepository,
        IPatientPurchasesRepository patientPurchasesRepository,
        IStockMovementsRepository stockMovementsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _patientApplicationsRepository = patientApplicationsRepository;
        _patientPurchasesRepository = patientPurchasesRepository;
        _stockMovementsRepository = stockMovementsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PatientApplicationDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var aplicacao = await _patientApplicationsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            empresaId,
            cancellationToken);

        if (aplicacao is null)
        {
            return Result<PatientApplicationDto>.Failure("Aplicação não encontrada.");
        }

        try
        {
            var dadosAnteriores = PatientApplicationsAuditSerializer.Serialize(aplicacao);
            var dataCancelamento = DateTime.UtcNow;

            aplicacao.Cancel();

            var saidas = aplicacao.MovimentacoesEstoque
                .Where(movimentacao => movimentacao.Tipo == TipoMovimentacaoEstoque.Saida)
                .ToList();

            var movimentacoesEstorno = new List<MovimentacaoEstoque>();

            foreach (var movimentacao in saidas)
            {
                var estorno = MovimentacaoEstoque.CreateEntradaFromCancelamentoAplicacao(
                    empresaId,
                    aplicacao.UnidadeId,
                    movimentacao.ProdutoId,
                    aplicacao.Id,
                    aplicacao.FuncionarioId,
                    movimentacao.Quantidade,
                    dataCancelamento);

                if (movimentacao.LoteProdutoId.HasValue)
                {
                    estorno.AssignLote(movimentacao.LoteProdutoId.Value);
                }

                movimentacoesEstorno.Add(estorno);
            }

            _patientApplicationsRepository.Update(aplicacao);
            if (movimentacoesEstorno.Count > 0)
            {
                await _stockMovementsRepository.AddRangeAsync(movimentacoesEstorno, cancellationToken);
            }

            if (aplicacao.CompraPacienteId.HasValue)
            {
                var compra = await _patientPurchasesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                    aplicacao.CompraPacienteId.Value,
                    empresaId,
                    cancellationToken);

                if (compra is not null)
                {
                    compra.ReopenIfCompleted();
                    _patientPurchasesRepository.Update(compra);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _patientApplicationsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                aplicacao.Id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(AplicacaoPaciente),
                aplicacao.Id,
                AcaoAuditoria.Cancelar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: PatientApplicationsAuditSerializer.Serialize(persisted ?? aplicacao),
                cancellationToken: cancellationToken);

            foreach (var movimentacao in movimentacoesEstorno)
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

            return Result<PatientApplicationDto>.Success(
                PatientApplicationsMapper.Map(persisted ?? aplicacao));
        }
        catch (DomainException exception)
        {
            return Result<PatientApplicationDto>.Failure(exception.Message);
        }
    }
}
