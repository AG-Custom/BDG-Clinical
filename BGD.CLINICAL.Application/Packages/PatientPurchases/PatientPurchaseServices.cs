using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Packages.Abstractions;
using BGD.CLINICAL.Application.Packages.Dtos;
using BGD.CLINICAL.Application.Packages.Packages;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Packages.PatientPurchases;

public interface ICreatePatientPurchasesService
{
    Task<Result<PatientPurchaseDto>> ExecuteAsync(
        CreatePatientPurchaseRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreatePatientPurchasesService : ICreatePatientPurchasesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;
    private readonly IPackagesRepository _packagesRepository;
    private readonly IPatientsRepository _patientsRepository;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePatientPurchasesService(
        ICurrentTenantContext tenantContext,
        IPatientPurchasesRepository patientPurchasesRepository,
        IPackagesRepository packagesRepository,
        IPatientsRepository patientsRepository,
        IUnitsRepository unitsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _patientPurchasesRepository = patientPurchasesRepository;
        _packagesRepository = packagesRepository;
        _patientsRepository = patientsRepository;
        _unitsRepository = unitsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PatientPurchaseDto>> ExecuteAsync(
        CreatePatientPurchaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        if (request.PacienteId == Guid.Empty)
        {
            return Result<PatientPurchaseDto>.Failure("Informe o paciente da compra.");
        }

        if (request.PacoteId == Guid.Empty)
        {
            return Result<PatientPurchaseDto>.Failure("Informe o pacote da compra.");
        }

        if (request.UnidadeId == Guid.Empty)
        {
            return Result<PatientPurchaseDto>.Failure("Informe a unidade da compra.");
        }

        if (!string.IsNullOrWhiteSpace(request.Observacao) && request.Observacao.Length > 2000)
        {
            return Result<PatientPurchaseDto>.Failure("A observação deve ter no máximo 2000 caracteres.");
        }

        var paciente = await _patientsRepository.GetByIdAndEmpresaIdAsync(
            request.PacienteId,
            empresaId,
            cancellationToken);

        if (paciente is null || !paciente.Ativo)
        {
            return Result<PatientPurchaseDto>.Failure("Paciente não encontrado ou inativo.");
        }

        var pacote = await _packagesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            request.PacoteId,
            empresaId,
            cancellationToken);

        if (pacote is null || !pacote.Ativo)
        {
            return Result<PatientPurchaseDto>.Failure("Pacote não encontrado ou inativo.");
        }

        var unidade = await _unitsRepository.GetByIdAndEmpresaIdAsync(
            request.UnidadeId,
            empresaId,
            cancellationToken);

        if (unidade is null || !unidade.Ativo)
        {
            return Result<PatientPurchaseDto>.Failure("Unidade não encontrada ou inativa.");
        }

        try
        {
            var compra = CompraPaciente.Create(
                empresaId,
                request.PacienteId,
                request.PacoteId,
                request.UnidadeId,
                request.DataCompra,
                request.Observacao);

            await _patientPurchasesRepository.AddAsync(compra, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _patientPurchasesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                compra.Id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(CompraPaciente),
                compra.Id,
                AcaoAuditoria.Criar,
                dadosNovos: PatientPurchasesAuditSerializer.Serialize(persisted ?? compra),
                cancellationToken: cancellationToken);

            return Result<PatientPurchaseDto>.Success(PatientPurchasesMapper.Map(persisted ?? compra));
        }
        catch (DomainException exception)
        {
            return Result<PatientPurchaseDto>.Failure(exception.Message);
        }
    }
}

public interface IListAllPatientPurchasesService
{
    Task<Result<IReadOnlyList<PatientPurchaseDto>>> ExecuteAsync(
        Guid? pacienteId,
        string? status,
        CancellationToken cancellationToken = default);
}

public sealed class ListAllPatientPurchasesService : IListAllPatientPurchasesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;
    private readonly IPatientsRepository _patientsRepository;

    public ListAllPatientPurchasesService(
        ICurrentTenantContext tenantContext,
        IPatientPurchasesRepository patientPurchasesRepository,
        IPatientsRepository patientsRepository)
    {
        _tenantContext = tenantContext;
        _patientPurchasesRepository = patientPurchasesRepository;
        _patientsRepository = patientsRepository;
    }

    public async Task<Result<IReadOnlyList<PatientPurchaseDto>>> ExecuteAsync(
        Guid? pacienteId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        StatusCompraPaciente? statusFiltro = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<StatusCompraPaciente>(status.Trim(), ignoreCase: true, out var parsed))
            {
                return Result<IReadOnlyList<PatientPurchaseDto>>.Failure("Status de compra inválido.");
            }

            statusFiltro = parsed;
        }

        if (pacienteId.HasValue && pacienteId.Value != Guid.Empty)
        {
            var paciente = await _patientsRepository.GetByIdAndEmpresaIdAsync(
                pacienteId.Value,
                _tenantContext.EmpresaId,
                cancellationToken);

            if (paciente is null)
            {
                return Result<IReadOnlyList<PatientPurchaseDto>>.Failure("Paciente não encontrado.");
            }
        }

        var compras = await _patientPurchasesRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            pacienteId.HasValue && pacienteId.Value != Guid.Empty ? pacienteId : null,
            statusFiltro,
            cancellationToken);

        return Result<IReadOnlyList<PatientPurchaseDto>>.Success(PatientPurchasesMapper.Map(compras));
    }
}

public interface IListPatientPurchasesService
{
    Task<Result<IReadOnlyList<PatientPurchaseDto>>> ExecuteAsync(
        Guid pacienteId,
        string? status,
        CancellationToken cancellationToken = default);
}

public sealed class ListPatientPurchasesService : IListPatientPurchasesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;
    private readonly IPatientsRepository _patientsRepository;

    public ListPatientPurchasesService(
        ICurrentTenantContext tenantContext,
        IPatientPurchasesRepository patientPurchasesRepository,
        IPatientsRepository patientsRepository)
    {
        _tenantContext = tenantContext;
        _patientPurchasesRepository = patientPurchasesRepository;
        _patientsRepository = patientsRepository;
    }

    public async Task<Result<IReadOnlyList<PatientPurchaseDto>>> ExecuteAsync(
        Guid pacienteId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        if (pacienteId == Guid.Empty)
        {
            return Result<IReadOnlyList<PatientPurchaseDto>>.Failure("Informe o paciente.");
        }

        StatusCompraPaciente? statusFiltro = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<StatusCompraPaciente>(status.Trim(), ignoreCase: true, out var parsed))
            {
                return Result<IReadOnlyList<PatientPurchaseDto>>.Failure("Status de compra inválido.");
            }

            statusFiltro = parsed;
        }

        var paciente = await _patientsRepository.GetByIdAndEmpresaIdAsync(
            pacienteId,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (paciente is null)
        {
            return Result<IReadOnlyList<PatientPurchaseDto>>.Failure("Paciente não encontrado.");
        }

        var compras = await _patientPurchasesRepository.ListByPacienteAsync(
            _tenantContext.EmpresaId,
            pacienteId,
            statusFiltro,
            cancellationToken);

        return Result<IReadOnlyList<PatientPurchaseDto>>.Success(PatientPurchasesMapper.Map(compras));
    }
}

public interface IListActivePatientPurchasesService
{
    Task<Result<IReadOnlyList<PatientPurchaseDto>>> ExecuteAsync(
        Guid pacienteId,
        CancellationToken cancellationToken = default);
}

public sealed class ListActivePatientPurchasesService : IListActivePatientPurchasesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;

    public ListActivePatientPurchasesService(
        ICurrentTenantContext tenantContext,
        IPatientPurchasesRepository patientPurchasesRepository)
    {
        _tenantContext = tenantContext;
        _patientPurchasesRepository = patientPurchasesRepository;
    }

    public async Task<Result<IReadOnlyList<PatientPurchaseDto>>> ExecuteAsync(
        Guid pacienteId,
        CancellationToken cancellationToken = default)
    {
        if (pacienteId == Guid.Empty)
        {
            return Result<IReadOnlyList<PatientPurchaseDto>>.Failure("Informe o paciente.");
        }

        var compras = await _patientPurchasesRepository.ListActiveByPacienteAsync(
            _tenantContext.EmpresaId,
            pacienteId,
            cancellationToken);

        return Result<IReadOnlyList<PatientPurchaseDto>>.Success(PatientPurchasesMapper.Map(compras));
    }
}

public interface IGetPatientPurchasesService
{
    Task<Result<PatientPurchaseDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class GetPatientPurchasesService : IGetPatientPurchasesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;

    public GetPatientPurchasesService(
        ICurrentTenantContext tenantContext,
        IPatientPurchasesRepository patientPurchasesRepository)
    {
        _tenantContext = tenantContext;
        _patientPurchasesRepository = patientPurchasesRepository;
    }

    public async Task<Result<PatientPurchaseDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var compra = await _patientPurchasesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (compra is null)
        {
            return Result<PatientPurchaseDto>.Failure("Compra de pacote não encontrada.");
        }

        return Result<PatientPurchaseDto>.Success(PatientPurchasesMapper.Map(compra));
    }
}

public interface IGetPatientPurchaseBalanceService
{
    Task<Result<PatientPurchaseBalanceDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetPatientPurchaseBalanceService : IGetPatientPurchaseBalanceService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;

    public GetPatientPurchaseBalanceService(
        ICurrentTenantContext tenantContext,
        IPatientPurchasesRepository patientPurchasesRepository)
    {
        _tenantContext = tenantContext;
        _patientPurchasesRepository = patientPurchasesRepository;
    }

    public async Task<Result<PatientPurchaseBalanceDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var compra = await _patientPurchasesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (compra is null)
        {
            return Result<PatientPurchaseBalanceDto>.Failure("Compra de pacote não encontrada.");
        }

        return Result<PatientPurchaseBalanceDto>.Success(PatientPurchasesMapper.MapBalance(compra));
    }
}

public interface ICancelPatientPurchasesService
{
    Task<Result<PatientPurchaseDto>> ExecuteAsync(
        Guid id,
        CancelPatientPurchaseRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CancelPatientPurchasesService : ICancelPatientPurchasesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPatientPurchasesRepository _patientPurchasesRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CancelPatientPurchasesService(
        ICurrentTenantContext tenantContext,
        IPatientPurchasesRepository patientPurchasesRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _patientPurchasesRepository = patientPurchasesRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PatientPurchaseDto>> ExecuteAsync(
        Guid id,
        CancelPatientPurchaseRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var compra = await _patientPurchasesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            empresaId,
            cancellationToken);

        if (compra is null)
        {
            return Result<PatientPurchaseDto>.Failure("Compra de pacote não encontrada.");
        }

        try
        {
            var dadosAnteriores = PatientPurchasesAuditSerializer.Serialize(compra);
            compra.Cancel(request.Observacao);
            _patientPurchasesRepository.Update(compra);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(CompraPaciente),
                compra.Id,
                AcaoAuditoria.Cancelar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: PatientPurchasesAuditSerializer.Serialize(compra),
                cancellationToken: cancellationToken);

            return Result<PatientPurchaseDto>.Success(PatientPurchasesMapper.Map(compra));
        }
        catch (DomainException exception)
        {
            return Result<PatientPurchaseDto>.Failure(exception.Message);
        }
    }
}
