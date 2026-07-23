namespace BGD.CLINICAL.Application.Inventory.Dtos;

public sealed record ProductTypeDto(
    Guid Id,
    string Nome,
    string? Codigo,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateProductTypeRequest(string Nome);

public sealed record UpdateProductTypeRequest(string Nome);
