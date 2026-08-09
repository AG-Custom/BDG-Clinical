namespace AG.CLINICAL.Application.Audits.Dtos;

public sealed record EntityAuditSummaryDto(
    Guid? IdUsuarioCriacao,
    Guid? IdUsuarioAtualizacao);
