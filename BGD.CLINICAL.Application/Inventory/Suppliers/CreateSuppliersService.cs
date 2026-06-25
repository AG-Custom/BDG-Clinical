using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Inventory.Suppliers;

public interface ICreateSuppliersService
{
    Task<Result<SupplierDto>> ExecuteAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateSuppliersService : ICreateSuppliersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISuppliersRepository _suppliersRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSuppliersService(
        ICurrentTenantContext tenantContext,
        ISuppliersRepository suppliersRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _suppliersRepository = suppliersRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SupplierDto>> ExecuteAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var validation = await SupplierRequestValidator.ValidateAsync(
            empresaId,
            request.Nome,
            request.Cnpj,
            request.Telefone,
            request.Email,
            excludeSupplierId: null,
            _suppliersRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<SupplierDto>.Failure(validation.Error!);
        }

        try
        {
            var data = validation.Value!;
            var fornecedor = Fornecedor.Create(
                empresaId,
                data.Nome,
                data.Cnpj,
                data.Telefone,
                data.Email);

            await _suppliersRepository.AddAsync(fornecedor, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Fornecedor),
                fornecedor.Id,
                AcaoAuditoria.Criar,
                dadosNovos: SuppliersAuditSerializer.Serialize(fornecedor),
                cancellationToken: cancellationToken);

            return Result<SupplierDto>.Success(SuppliersMapper.Map(fornecedor));
        }
        catch (DomainException exception)
        {
            return Result<SupplierDto>.Failure(exception.Message);
        }
    }
}
