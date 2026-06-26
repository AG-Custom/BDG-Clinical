using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Inventory.Products;

public interface IUpdateProductsService
{
    Task<Result<ProductDto>> ExecuteAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateProductsService : IUpdateProductsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IProductsRepository _productsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductsService(
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
        UpdateProductRequest request,
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
            return Result<ProductDto>.Failure("Não é possível editar um produto inativo.");
        }

        var validation = await ProductRequestValidator.ValidateAsync(
            empresaId,
            request.TipoProdutoId,
            request.UnidadeMedidaId,
            request.Nome,
            request.EstoqueMinimo,
            request.Sku,
            request.CodigoInterno,
            request.CodigoBarras,
            request.ControlaEstoque,
            excludeProductId: id,
            _productsRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<ProductDto>.Failure(validation.Error!);
        }

        try
        {
            var dadosAnteriores = ProductsAuditSerializer.Serialize(produto);
            var data = validation.Value!;

            produto.UpdateDetails(
                data.TipoProdutoId,
                data.UnidadeMedidaId,
                data.Nome,
                data.EstoqueMinimo,
                data.Sku,
                data.CodigoInterno,
                data.CodigoBarras,
                data.ControlaEstoque);

            _productsRepository.Update(produto);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _productsRepository.GetByIdAndEmpresaIdAsync(produto.Id, empresaId, cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(Produto),
                produto.Id,
                AcaoAuditoria.Editar,
                dadosAnteriores: dadosAnteriores,
                dadosNovos: ProductsAuditSerializer.Serialize(persisted ?? produto),
                cancellationToken: cancellationToken);

            return Result<ProductDto>.Success(ProductsMapper.Map(persisted ?? produto));
        }
        catch (DomainException exception)
        {
            return Result<ProductDto>.Failure(exception.Message);
        }
    }
}
