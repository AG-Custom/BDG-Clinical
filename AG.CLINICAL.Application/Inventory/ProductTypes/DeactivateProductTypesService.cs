using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Inventory.ProductTypes;

public interface IDeactivateProductTypesService
{
    Task<Result<ProductTypeDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class DeactivateProductTypesService : IDeactivateProductTypesService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProductTypesRepository _productTypesRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateProductTypesService(
        ICurrentTenantContext tenantContext,
        IProductTypesRepository productTypesRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _productTypesRepository = productTypesRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductTypeDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var tipoProduto = await _productTypesRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (tipoProduto is null)
        {
            return Result<ProductTypeDto>.Failure("Tipo de produto não encontrado.");
        }

        if (!tipoProduto.Ativo)
        {
            return Result<ProductTypeDto>.Failure("Tipo de produto já está inativo.");
        }

        if (tipoProduto.EhTipoSistema)
        {
            return Result<ProductTypeDto>.Failure("Tipos de produto padrão do sistema não podem ser excluídos.");
        }

        var dadosAnteriores = ProductTypesAuditSerializer.Serialize(tipoProduto);

        try
        {
            tipoProduto.Deactivate();
        }
        catch (DomainException exception)
        {
            return Result<ProductTypeDto>.Failure(exception.Message);
        }

        _productTypesRepository.Update(tipoProduto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(TipoProduto),
            tipoProduto.Id,
            AcaoAuditoria.Excluir,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: ProductTypesAuditSerializer.Serialize(tipoProduto),
            cancellationToken: cancellationToken);

        return Result<ProductTypeDto>.Success(ProductTypesMapper.Map(tipoProduto));
    }
}
