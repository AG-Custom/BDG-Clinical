using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Packages.Abstractions;
using BGD.CLINICAL.Application.Packages.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Packages.Packages;

public interface ICreatePackagesService
{
    Task<Result<PackageDto>> ExecuteAsync(
        CreatePackageRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreatePackagesService : ICreatePackagesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPackagesRepository _packagesRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePackagesService(
        ICurrentTenantContext tenantContext,
        IPackagesRepository packagesRepository,
        IProductsRepository productsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _packagesRepository = packagesRepository;
        _productsRepository = productsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PackageDto>> ExecuteAsync(
        CreatePackageRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var validation = await PackageRequestValidator.ValidateAsync(
            empresaId,
            request.Nome,
            request.Descricao,
            request.Valor,
            request.Itens,
            excludePackageId: null,
            _packagesRepository,
            _productsRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<PackageDto>.Failure(validation.Error!);
        }

        try
        {
            var data = validation.Value!;
            var pacote = Pacote.Create(
                empresaId,
                data.Nome,
                data.Descricao,
                data.Valor);
            pacote.ReplaceItems(data.Itens);

            await _packagesRepository.AddAsync(pacote, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _packagesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                pacote.Id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Pacote),
                pacote.Id,
                AcaoAuditoria.Criar,
                dadosNovos: PackagesAuditSerializer.Serialize(persisted ?? pacote),
                cancellationToken: cancellationToken);

            return Result<PackageDto>.Success(PackagesMapper.Map(persisted ?? pacote));
        }
        catch (DomainException exception)
        {
            return Result<PackageDto>.Failure(exception.Message);
        }
    }
}

public interface IListPackagesService
{
    Task<Result<IReadOnlyList<PackageDto>>> ExecuteAsync(
        bool includeInactive,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default);
}

public sealed class ListPackagesService : IListPackagesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPackagesRepository _packagesRepository;

    public ListPackagesService(
        ICurrentTenantContext tenantContext,
        IPackagesRepository packagesRepository)
    {
        _tenantContext = tenantContext;
        _packagesRepository = packagesRepository;
    }

    public async Task<Result<IReadOnlyList<PackageDto>>> ExecuteAsync(
        bool includeInactive,
        string? search,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        string? normalizedSearch = null;
        int? effectiveLimit = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchResult = PackageRequestValidator.ValidateSearch(search, limit);
            if (searchResult.IsFailure)
            {
                return Result<IReadOnlyList<PackageDto>>.Failure(searchResult.Error!);
            }

            (normalizedSearch, var validatedLimit) = searchResult.Value!;
            effectiveLimit = validatedLimit;
        }
        else if (limit.HasValue)
        {
            var limitResult = PackageRequestValidator.ValidateLimit(limit, PackageSearchOptions.DefaultLimit);
            if (limitResult.IsFailure)
            {
                return Result<IReadOnlyList<PackageDto>>.Failure(limitResult.Error!);
            }

            effectiveLimit = limitResult.Value;
        }

        var pacotes = await _packagesRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            includeInactive,
            normalizedSearch,
            effectiveLimit,
            cancellationToken);

        return Result<IReadOnlyList<PackageDto>>.Success(PackagesMapper.Map(pacotes));
    }
}

public interface IGetPackagesService
{
    Task<Result<PackageDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class GetPackagesService : IGetPackagesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPackagesRepository _packagesRepository;

    public GetPackagesService(
        ICurrentTenantContext tenantContext,
        IPackagesRepository packagesRepository)
    {
        _tenantContext = tenantContext;
        _packagesRepository = packagesRepository;
    }

    public async Task<Result<PackageDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var pacote = await _packagesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (pacote is null)
        {
            return Result<PackageDto>.Failure("Pacote não encontrado.");
        }

        return Result<PackageDto>.Success(PackagesMapper.Map(pacote));
    }
}

public interface IUpdatePackagesService
{
    Task<Result<PackageDto>> ExecuteAsync(
        Guid id,
        UpdatePackageRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdatePackagesService : IUpdatePackagesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPackagesRepository _packagesRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePackagesService(
        ICurrentTenantContext tenantContext,
        IPackagesRepository packagesRepository,
        IProductsRepository productsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _packagesRepository = packagesRepository;
        _productsRepository = productsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PackageDto>> ExecuteAsync(
        Guid id,
        UpdatePackageRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var pacote = await _packagesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            empresaId,
            cancellationToken);

        if (pacote is null)
        {
            return Result<PackageDto>.Failure("Pacote não encontrado.");
        }

        var validation = await PackageRequestValidator.ValidateAsync(
            empresaId,
            request.Nome,
            request.Descricao,
            request.Valor,
            request.Itens,
            excludePackageId: id,
            _packagesRepository,
            _productsRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<PackageDto>.Failure(validation.Error!);
        }

        try
        {
            var dadosAnteriores = PackagesAuditSerializer.Serialize(pacote);
            var data = validation.Value!;
            pacote.UpdateDetails(data.Nome, data.Descricao, data.Valor);
            pacote.ReplaceItems(data.Itens);
            _packagesRepository.Update(pacote);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _packagesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Pacote),
                pacote.Id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: PackagesAuditSerializer.Serialize(persisted ?? pacote),
                cancellationToken: cancellationToken);

            return Result<PackageDto>.Success(PackagesMapper.Map(persisted ?? pacote));
        }
        catch (DomainException exception)
        {
            return Result<PackageDto>.Failure(exception.Message);
        }
    }
}

public interface IDeactivatePackagesService
{
    Task<Result<PackageDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class DeactivatePackagesService : IDeactivatePackagesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPackagesRepository _packagesRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePackagesService(
        ICurrentTenantContext tenantContext,
        IPackagesRepository packagesRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _packagesRepository = packagesRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PackageDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var pacote = await _packagesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            empresaId,
            cancellationToken);

        if (pacote is null)
        {
            return Result<PackageDto>.Failure("Pacote não encontrado.");
        }

        if (!pacote.Ativo)
        {
            return Result<PackageDto>.Failure("Pacote já está inativo.");
        }

        var dadosAnteriores = PackagesAuditSerializer.Serialize(pacote);
        pacote.Deactivate();
        _packagesRepository.Update(pacote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Pacote),
            pacote.Id,
            AcaoAuditoria.Excluir,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: PackagesAuditSerializer.Serialize(pacote),
            cancellationToken: cancellationToken);

        return Result<PackageDto>.Success(PackagesMapper.Map(pacote));
    }
}

public interface IReactivatePackagesService
{
    Task<Result<PackageDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default);
}

public sealed class ReactivatePackagesService : IReactivatePackagesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IPackagesRepository _packagesRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivatePackagesService(
        ICurrentTenantContext tenantContext,
        IPackagesRepository packagesRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _packagesRepository = packagesRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PackageDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var pacote = await _packagesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
            id,
            empresaId,
            cancellationToken);

        if (pacote is null)
        {
            return Result<PackageDto>.Failure("Pacote não encontrado.");
        }

        if (pacote.Ativo)
        {
            return Result<PackageDto>.Failure("Pacote já está ativo.");
        }

        var dadosAnteriores = PackagesAuditSerializer.Serialize(pacote);
        pacote.Reactivate();
        _packagesRepository.Update(pacote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Pacote),
            pacote.Id,
            AcaoAuditoria.Editar,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: PackagesAuditSerializer.Serialize(pacote),
            cancellationToken: cancellationToken);

        return Result<PackageDto>.Success(PackagesMapper.Map(pacote));
    }
}
