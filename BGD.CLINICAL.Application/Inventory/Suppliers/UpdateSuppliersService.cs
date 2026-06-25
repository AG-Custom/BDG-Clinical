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

public interface IUpdateSuppliersService
{
    Task<Result<SupplierDto>> ExecuteAsync(
        Guid id,
        UpdateSupplierRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateSuppliersService : IUpdateSuppliersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISuppliersRepository _suppliersRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSuppliersService(
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
        Guid id,
        UpdateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var fornecedor = await _suppliersRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (fornecedor is null)
        {
            return Result<SupplierDto>.Failure("Fornecedor não encontrado.");
        }

        if (!fornecedor.Ativo)
        {
            return Result<SupplierDto>.Failure("Não é possível editar um fornecedor inativo.");
        }

        var validation = await SupplierRequestValidator.ValidateAsync(
            empresaId,
            request.Nome,
            request.Cnpj,
            request.Telefone,
            request.Email,
            excludeSupplierId: id,
            _suppliersRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<SupplierDto>.Failure(validation.Error!);
        }

        try
        {
            var dadosAnteriores = SuppliersAuditSerializer.Serialize(fornecedor);
            var data = validation.Value!;

            fornecedor.UpdateDetails(data.Nome, data.Cnpj, data.Telefone, data.Email);
            _suppliersRepository.Update(fornecedor);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Fornecedor),
                fornecedor.Id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
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
