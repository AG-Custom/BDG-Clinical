namespace BGD.CLINICAL.Application.Audits.Dtos;

public sealed record EntityAuditSummaryDto(
    Guid? IdUsuarioCriacao,
    Guid? IdUsuarioAtualizacao);
