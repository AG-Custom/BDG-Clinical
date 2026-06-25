using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Inventory.SupplierOrders;

public interface ICreateSupplierOrdersService
{
    Task<Result<SupplierOrderDto>> ExecuteAsync(
        CreateSupplierOrderRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateSupplierOrdersService : ICreateSupplierOrdersService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ISupplierOrdersRepository _supplierOrdersRepository;
    private readonly ISuppliersRepository _suppliersRepository;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupplierOrdersService(
        ICurrentTenantContext tenantContext,
        ISupplierOrdersRepository supplierOrdersRepository,
        ISuppliersRepository suppliersRepository,
        IUnitsRepository unitsRepository,
        IProductsRepository productsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _supplierOrdersRepository = supplierOrdersRepository;
        _suppliersRepository = suppliersRepository;
        _unitsRepository = unitsRepository;
        _productsRepository = productsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SupplierOrderDto>> ExecuteAsync(
        CreateSupplierOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        var validation = await SupplierOrderRequestValidator.ValidateAsync(
            empresaId,
            request.FornecedorId,
            request.UnidadeId,
            request.TipoPedido,
            request.DataPedido,
            request.Status,
            request.Observacao,
            request.Itens,
            _suppliersRepository,
            _unitsRepository,
            _productsRepository,
            cancellationToken);

        if (validation.IsFailure)
        {
            return Result<SupplierOrderDto>.Failure(validation.Error!);
        }

        try
        {
            var data = validation.Value!;
            var pedido = PedidoFornecedor.Create(
                empresaId,
                data.FornecedorId,
                data.UnidadeId,
                data.TipoPedido,
                data.DataPedido,
                data.Status,
                data.Observacao,
                data.Itens);

            await _supplierOrdersRepository.AddAsync(pedido, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var persisted = await _supplierOrdersRepository.GetByIdAndEmpresaIdWithItensAsync(
                pedido.Id,
                empresaId,
                cancellationToken);

            await _auditLogsService.RegisterEntityChangeAsync(
                empresaId,
                _tenantContext.UsuarioId,
                nameof(PedidoFornecedor),
                pedido.Id,
                AcaoAuditoria.Criar,
                dadosNovos: SupplierOrdersAuditSerializer.Serialize(persisted ?? pedido),
                cancellationToken: cancellationToken);

            return Result<SupplierOrderDto>.Success(SupplierOrdersMapper.Map(persisted ?? pedido));
        }
        catch (DomainException exception)
        {
            return Result<SupplierOrderDto>.Failure(exception.Message);
        }
    }
}
