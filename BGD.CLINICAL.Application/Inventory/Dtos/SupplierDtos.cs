namespace BGD.CLINICAL.Application.Inventory.Dtos;

public sealed record SupplierDto(
    Guid Id,
    string Nome,
    string Cnpj,
    string? Telefone,
    string? Email,
    string? Observacao,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateSupplierRequest(
    string Nome,
    string Cnpj,
    string? Telefone,
    string? Email,
    string? Observacao = null);

public sealed record UpdateSupplierRequest(
    string Nome,
    string Cnpj,
    string? Telefone,
    string? Email,
    string? Observacao = null);

public sealed record SupplierOrderAttachmentDto(
    Guid Id,
    string NomeArquivo,
    string ContentType,
    string Url,
    long TamanhoBytes,
    DateTime CriadoEm);
