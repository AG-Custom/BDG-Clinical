using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Inventory.Products;

public interface IDeactivateProductsService
{
    Task<Result<ProductDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class DeactivateProductsService : IDeactivateProductsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProductsRepository _productsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateProductsService(
        ICurrentTenantContext tenantContext,
        IProductsRepository productsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _productsRepository = productsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var produto = await _productsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (produto is null)
        {
            return Result<ProductDto>.Failure("Produto não encontrado.");
        }

        if (!produto.Ativo)
        {
            return Result<ProductDto>.Failure("Produto já está inativo.");
        }

        var dadosAnteriores = ProductsAuditSerializer.Serialize(produto);

        produto.Deactivate();
        _productsRepository.Update(produto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Produto),
            produto.Id,
            AcaoAuditoria.Excluir,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: ProductsAuditSerializer.Serialize(produto),
            cancellationToken: cancellationToken);

        return Result<ProductDto>.Success(ProductsMapper.Map(produto));
    }
}
