using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Inventory.Abstractions;
using AG.CLINICAL.Application.Inventory.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Inventory.StockMovements;

public interface ICreateStockTransferService
{
    Task<Result<StockTransferDto>> ExecuteAsync(
        CreateStockTransferRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateStockTransferService : ICreateStockTransferService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUnitsRepository _unitsRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IStockBalancesRepository _stockBalancesRepository;
    private readonly IProductLotsRepository _productLotsRepository;
    private readonly IStockMovementsRepository _stockMovementsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IMedicationLotStockService _medicationLotStockService;
    private readonly IAuditLogsService _auditLogsService;

    public CreateStockTransferService(
        ICurrentTenantContext tenantContext,
        IUnitsRepository unitsRepository,
        IProductsRepository productsRepository,
        IStockBalancesRepository stockBalancesRepository,
        IProductLotsRepository productLotsRepository,
        IStockMovementsRepository stockMovementsRepository,
        IUsersRepository usersRepository,
        IMedicationLotStockService medicationLotStockService,
        IAuditLogsService auditLogsService)
    {
        _tenantContext = tenantContext;
        _unitsRepository = unitsRepository;
        _productsRepository = productsRepository;
        _stockBalancesRepository = stockBalancesRepository;
        _productLotsRepository = productLotsRepository;
        _stockMovementsRepository = stockMovementsRepository;
        _usersRepository = usersRepository;
        _medicationLotStockService = medicationLotStockService;
        _auditLogsService = auditLogsService;
    }

    public async Task<Result<StockTransferDto>> ExecuteAsync(
        CreateStockTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;

        try
        {
            var validationError = ValidateRequest(request);
            if (validationError is not null)
            {
                return Result<StockTransferDto>.Failure(validationError);
            }

            var unidadeOrigem = await _unitsRepository.GetByIdAndEmpresaIdAsync(
                request.UnidadeOrigemId,
                empresaId,
                cancellationToken);
            var unidadeDestino = await _unitsRepository.GetByIdAndEmpresaIdAsync(
                request.UnidadeDestinoId,
                empresaId,
                cancellationToken);

            if (unidadeOrigem is null)
            {
                return Result<StockTransferDto>.Failure("Unidade de origem não encontrada.");
            }

            if (unidadeDestino is null)
            {
                return Result<StockTransferDto>.Failure("Unidade de destino não encontrada.");
            }

            if (!unidadeOrigem.Ativo)
            {
                return Result<StockTransferDto>.Failure("A unidade de origem está inativa.");
            }

            if (!unidadeDestino.Ativo)
            {
                return Result<StockTransferDto>.Failure("A unidade de destino está inativa.");
            }

            var produto = await _productsRepository.GetByIdAndEmpresaIdAsync(
                request.ProdutoId,
                empresaId,
                cancellationToken);

            if (produto is null || !produto.Ativo)
            {
                return Result<StockTransferDto>.Failure("Produto não encontrado ou inativo.");
            }

            if (!produto.ControlaEstoque)
            {
                return Result<StockTransferDto>.Failure("O produto informado não controla estoque.");
            }

            var saldo = await _stockBalancesRepository.GetSaldoByUnidadeAndProdutoAsync(
                empresaId,
                request.UnidadeOrigemId,
                request.ProdutoId,
                cancellationToken);

            if (saldo < request.Quantidade)
            {
                return Result<StockTransferDto>.Failure(
                    $"Saldo insuficiente na unidade de origem. Saldo disponível: {saldo}; quantidade solicitada: {request.Quantidade}.");
            }

            var saldoOrigem = (await _stockBalancesRepository.ListByEmpresaIdAsync(
                empresaId,
                request.UnidadeOrigemId,
                request.ProdutoId,
                abaixoDoMinimo: null,
                search: null,
                limit: 1,
                cancellationToken)).FirstOrDefault();
            var valorUnitarioOrigem = saldoOrigem?.ValorUnitario
                ?? ProductStockValuation.ResolveValorPorUnidadeEstoque(
                    produto.Valor,
                    produto.FatorEmbalagemParaEstoque);

            var usuario = await _usersRepository.GetByIdAsync(
                _tenantContext.UsuarioId,
                cancellationToken);
            var funcionarioId = usuario?.FuncionarioId;
            var transferenciaId = Guid.NewGuid();
            var observacao = string.IsNullOrWhiteSpace(request.Observacao)
                ? null
                : request.Observacao.Trim();
            var movimentacoes = new List<MovimentacaoEstoque>();
            var requisitos = new List<StockTransferBalanceRequirement>
            {
                new(request.UnidadeOrigemId, request.ProdutoId, null, request.Quantidade),
            };

            if (_medicationLotStockService.RequiresLot(produto))
            {
                var alocacoes = await _medicationLotStockService.AllocateFefoAsync(
                    empresaId,
                    request.UnidadeOrigemId,
                    produto,
                    request.Quantidade,
                    cancellationToken);

                foreach (var alocacao in alocacoes)
                {
                    var loteOrigem = await _productLotsRepository.GetByIdAndEmpresaIdAsync(
                        alocacao.LoteProdutoId,
                        empresaId,
                        cancellationToken)
                        ?? throw new DomainException("Lote de origem não encontrado.");

                    if (!loteOrigem.Ativo)
                    {
                        throw new DomainException($"O lote {loteOrigem.Codigo} está inativo e não pode ser transferido.");
                    }

                    if (loteOrigem.DataValidade < DateOnly.FromDateTime(DateTime.UtcNow))
                    {
                        throw new DomainException($"O lote {loteOrigem.Codigo} está vencido e não pode ser transferido.");
                    }

                    var loteDestino = await _productLotsRepository.GetByCodigoAsync(
                        empresaId,
                        request.UnidadeDestinoId,
                        request.ProdutoId,
                        loteOrigem.Codigo,
                        cancellationToken);

                    if (loteDestino is not null && loteDestino.DataValidade != loteOrigem.DataValidade)
                    {
                        throw new DomainException(
                            $"O lote {loteOrigem.Codigo} já existe no destino com outra data de validade.");
                    }

                    if (loteDestino is not null && !loteDestino.Ativo)
                    {
                        throw new DomainException($"O lote {loteDestino.Codigo} está inativo na unidade de destino.");
                    }

                    if (loteDestino is null)
                    {
                        loteDestino = LoteProduto.Create(
                            empresaId,
                            request.UnidadeDestinoId,
                            request.ProdutoId,
                            loteOrigem.Codigo,
                            loteOrigem.DataValidade);
                        await _productLotsRepository.AddAsync(loteDestino, cancellationToken);
                    }

                    var saida = CreateMovement(
                        isEntry: false,
                        transferenciaId,
                        empresaId,
                        request.UnidadeOrigemId,
                        request.ProdutoId,
                        alocacao.Quantidade,
                        request.Data,
                        funcionarioId,
                        observacao);
                    saida.AssignLote(loteOrigem.Id);

                    var entrada = CreateMovement(
                        isEntry: true,
                        transferenciaId,
                        empresaId,
                        request.UnidadeDestinoId,
                        request.ProdutoId,
                        alocacao.Quantidade,
                        request.Data,
                        funcionarioId,
                        observacao);
                    entrada.AssignLote(loteDestino.Id);

                    movimentacoes.Add(saida);
                    movimentacoes.Add(entrada);
                    requisitos.Add(new StockTransferBalanceRequirement(
                        request.UnidadeOrigemId,
                        request.ProdutoId,
                        loteOrigem.Id,
                        alocacao.Quantidade));
                }
            }
            else
            {
                movimentacoes.Add(CreateMovement(
                    isEntry: false,
                    transferenciaId,
                    empresaId,
                    request.UnidadeOrigemId,
                    request.ProdutoId,
                    request.Quantidade,
                    request.Data,
                    funcionarioId,
                    observacao));
                movimentacoes.Add(CreateMovement(
                    isEntry: true,
                    transferenciaId,
                    empresaId,
                    request.UnidadeDestinoId,
                    request.ProdutoId,
                    request.Quantidade,
                    request.Data,
                    funcionarioId,
                    observacao));
            }

            foreach (var movimentacao in movimentacoes)
            {
                movimentacao.AssignValorUnitario(valorUnitarioOrigem);
            }

            await _stockMovementsRepository.AddTransferAtomicallyAsync(
                empresaId,
                movimentacoes,
                requisitos,
                cancellationToken);

            var dtos = new List<StockMovementDto>();
            foreach (var movimentacao in movimentacoes)
            {
                await _auditLogsService.RegisterEntityChangeAsync(
                    empresaId,
                    _tenantContext.UsuarioId,
                    nameof(MovimentacaoEstoque),
                    movimentacao.Id,
                    AcaoAuditoria.GerarMovimentacao,
                    dadosNovos: StockMovementsAuditSerializer.Serialize(movimentacao),
                    cancellationToken: cancellationToken);

                var persisted = await _stockMovementsRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                    movimentacao.Id,
                    empresaId,
                    cancellationToken);
                dtos.Add(StockMovementsMapper.Map(persisted ?? movimentacao));
            }

            return Result<StockTransferDto>.Success(new StockTransferDto(
                transferenciaId,
                unidadeOrigem.Id,
                unidadeOrigem.Nome,
                unidadeDestino.Id,
                unidadeDestino.Nome,
                produto.Id,
                produto.Nome,
                request.Quantidade,
                request.Data,
                observacao,
                dtos));
        }
        catch (DomainException exception)
        {
            return Result<StockTransferDto>.Failure(exception.Message);
        }
    }

    private static string? ValidateRequest(CreateStockTransferRequest request)
    {
        if (request.UnidadeOrigemId == Guid.Empty)
        {
            return "Informe a unidade de origem.";
        }

        if (request.UnidadeDestinoId == Guid.Empty)
        {
            return "Informe a unidade de destino.";
        }

        if (request.UnidadeOrigemId == request.UnidadeDestinoId)
        {
            return "A unidade de destino deve ser diferente da unidade de origem.";
        }

        if (request.ProdutoId == Guid.Empty)
        {
            return "Informe o produto.";
        }

        if (request.Quantidade <= 0)
        {
            return "A quantidade deve ser maior que zero.";
        }

        if (!string.IsNullOrWhiteSpace(request.Observacao) && request.Observacao.Length > 2000)
        {
            return "A observação deve ter no máximo 2000 caracteres.";
        }

        return null;
    }

    private static MovimentacaoEstoque CreateMovement(
        bool isEntry,
        Guid transferenciaId,
        Guid empresaId,
        Guid unidadeId,
        Guid produtoId,
        decimal quantidade,
        DateTime data,
        Guid? funcionarioId,
        string? observacao)
    {
        return isEntry
            ? MovimentacaoEstoque.CreateTransferenciaEntrada(
                transferenciaId, empresaId, unidadeId, produtoId, quantidade, data, funcionarioId, observacao)
            : MovimentacaoEstoque.CreateTransferenciaSaida(
                transferenciaId, empresaId, unidadeId, produtoId, quantidade, data, funcionarioId, observacao);
    }
}
