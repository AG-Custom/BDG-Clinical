namespace BGD.CLINICAL.Application.Inventory.Dtos;

public sealed record StockMovementDto(
    Guid Id,
    Guid UnidadeId,
    string UnidadeNome,
    Guid ProdutoId,
    string ProdutoNome,
    string Tipo,
    decimal Quantidade,
    DateTime Data,
    string Origem,
    Guid? PedidoFornecedorId,
    Guid? AplicacaoPacienteId,
    string? Observacao,
    DateTime CriadoEm);
