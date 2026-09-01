using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Schedules.AppointmentTags;

public interface IDeactivateAppointmentTagsService
{
    Task<Result<AppointmentTagCatalogDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class DeactivateAppointmentTagsService : IDeactivateAppointmentTagsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAppointmentTagsRepository _appointmentTagsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAppointmentTagsService(
        ICurrentTenantContext tenantContext,
        IAppointmentTagsRepository appointmentTagsRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _appointmentTagsRepository = appointmentTagsRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AppointmentTagCatalogDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var empresaId = _tenantContext.EmpresaId;
        var tag = await _appointmentTagsRepository.GetByIdAndEmpresaIdAsync(id, empresaId, cancellationToken);

        if (tag is null)
        {
            return Result<AppointmentTagCatalogDto>.Failure("Tag não encontrada.");
        }

        if (!tag.Ativo)
        {
            return Result<AppointmentTagCatalogDto>.Failure("Tag já está inativa.");
        }

        var dadosAnteriores = AppointmentTagsAuditSerializer.Serialize(tag);

        tag.Deactivate();
        _appointmentTagsRepository.Update(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(TagAgendamento),
            tag.Id,
            AcaoAuditoria.Excluir,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: AppointmentTagsAuditSerializer.Serialize(tag),
            cancellationToken: cancellationToken);

        return Result<AppointmentTagCatalogDto>.Success(AppointmentTagsMapper.Map(tag));
    }
}
