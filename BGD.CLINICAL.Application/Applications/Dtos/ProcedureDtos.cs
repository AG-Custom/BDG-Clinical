namespace BGD.CLINICAL.Application.Applications.Dtos;

public sealed record ProcedureItemDto(
    Guid Id,
    Guid ProdutoId,
    string ProdutoNome,
    decimal Quantidade);

public sealed record ProcedureDto(
    Guid Id,
    string Nome,
    Guid? ProdutoAplicadoId,
    string? ProdutoAplicadoNome,
    string? Observacoes,
    bool Ativo,
    IReadOnlyList<ProcedureItemDto> Itens,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateProcedureItemRequest(
    Guid ProdutoId,
    decimal Quantidade);

public sealed record CreateProcedureRequest(
    string Nome,
    Guid? ProdutoAplicadoId,
    string? Observacoes,
    IReadOnlyList<CreateProcedureItemRequest>? Itens);

public sealed record UpdateProcedureRequest(
    string Nome,
    Guid? ProdutoAplicadoId,
    string? Observacoes,
    IReadOnlyList<CreateProcedureItemRequest>? Itens);
