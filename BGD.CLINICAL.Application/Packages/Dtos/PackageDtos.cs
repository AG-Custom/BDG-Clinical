using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Packages.Dtos;

public sealed record PackageItemDto(
    Guid Id,
    Guid ProdutoId,
    string ProdutoNome,
    decimal QuantidadeTotal,
    string UnidadeMedida);

public sealed record PackageDto(
    Guid Id,
    string Nome,
    string? Descricao,
    decimal Valor,
    bool Ativo,
    IReadOnlyList<PackageItemDto> Itens,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreatePackageItemRequest(
    Guid ProdutoId,
    decimal QuantidadeTotal,
    string UnidadeMedida);

public sealed record CreatePackageRequest(
    string Nome,
    string? Descricao,
    decimal Valor,
    IReadOnlyList<CreatePackageItemRequest> Itens);

public sealed record UpdatePackageRequest(
    string Nome,
    string? Descricao,
    decimal Valor,
    IReadOnlyList<CreatePackageItemRequest> Itens);

public sealed record PatientPurchaseProductBalanceDto(
    Guid ProdutoId,
    string ProdutoNome,
    string UnidadeMedida,
    decimal QuantidadeContratada,
    decimal QuantidadeUtilizada,
    decimal QuantidadeRestante);

public sealed record PatientPurchaseBalanceDto(
    Guid CompraPacienteId,
    IReadOnlyList<PatientPurchaseProductBalanceDto> Produtos);

public sealed record PatientPurchaseDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNome,
    Guid PacoteId,
    string PacoteNome,
    Guid UnidadeId,
    string UnidadeNome,
    DateTime DataCompra,
    StatusCompraPaciente Status,
    string? Observacao,
    PatientPurchaseBalanceDto Saldo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreatePatientPurchaseRequest(
    Guid PacienteId,
    Guid PacoteId,
    Guid UnidadeId,
    DateTime DataCompra,
    string? Observacao = null);

public sealed record CancelPatientPurchaseRequest(
    string? Observacao = null);
