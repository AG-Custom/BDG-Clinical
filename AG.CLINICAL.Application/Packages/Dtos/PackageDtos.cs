using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Packages.Dtos;

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

public sealed record UpdatePatientPurchaseBalanceItemRequest(
    Guid ProdutoId,
    decimal QuantidadeContratada,
    decimal QuantidadeUtilizada);

public sealed record UpdatePatientPurchaseBalanceRequest(
    IReadOnlyList<UpdatePatientPurchaseBalanceItemRequest> Itens,
    string? Motivo = null);

public sealed record PatientPurchaseHistoryEventDto(
    string Tipo,
    DateTime Data,
    Guid? ProdutoId = null,
    string? ProdutoNome = null,
    decimal? Quantidade = null,
    decimal? QuantidadeAnterior = null,
    decimal? QuantidadeNova = null,
    string? CampoAjuste = null,
    string? UnidadeMedida = null,
    Guid? AplicadorId = null,
    string? AplicadorNome = null,
    Guid? UsuarioId = null,
    string? UsuarioNome = null,
    Guid? LoteProdutoId = null,
    string? LoteCodigo = null,
    Guid? AplicacaoId = null,
    string? Motivo = null,
    bool? Cancelada = null);

public sealed record PatientPurchaseHistoryDto(
    Guid CompraPacienteId,
    IReadOnlyList<PatientPurchaseHistoryEventDto> Eventos);

public sealed record ReconcilePatientPurchaseItemRowDto(
    Guid PacienteId,
    string PacienteNome,
    Guid CompraPacienteId,
    string PacoteNome,
    Guid ProdutoId,
    string ProdutoNome,
    decimal Contratado,
    decimal AplicacoesValidas,
    decimal BaseReconstruida,
    decimal RestanteNovo,
    bool Divergencia,
    string? MotivoDivergencia);

public sealed record ReconcilePatientPurchaseItemsResultDto(
    int ComprasProcessadas,
    int ComprasJaMigradas,
    int ItensCriados,
    int Divergencias,
    bool Persistido,
    IReadOnlyList<ReconcilePatientPurchaseItemRowDto> Relatorio);
