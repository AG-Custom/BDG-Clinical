namespace BGD.CLINICAL.Application.Inventory.Dtos;

public sealed record SupplierDto(
    Guid Id,
    string Nome,
    string Cnpj,
    string? Telefone,
    string? Email,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateSupplierRequest(
    string Nome,
    string Cnpj,
    string? Telefone,
    string? Email);

public sealed record UpdateSupplierRequest(
    string Nome,
    string Cnpj,
    string? Telefone,
    string? Email);
