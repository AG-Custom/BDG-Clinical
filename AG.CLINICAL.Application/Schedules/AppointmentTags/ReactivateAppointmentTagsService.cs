using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;

namespace AG.CLINICAL.Application.Schedules.AppointmentTags;

public interface IReactivateAppointmentTagsService
{
    Task<Result<AppointmentTagCatalogDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class ReactivateAppointmentTagsService : IReactivateAppointmentTagsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAppointmentTagsRepository _appointmentTagsRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivateAppointmentTagsService(
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

        if (tag.Ativo)
        {
            return Result<AppointmentTagCatalogDto>.Failure("Tag já está ativa.");
        }

        if (await _appointmentTagsRepository.ExistsByNomeAsync(empresaId, tag.Nome, tag.Id, cancellationToken))
        {
            return Result<AppointmentTagCatalogDto>.Failure("Já existe uma tag com este nome.");
        }

        var dadosAnteriores = AppointmentTagsAuditSerializer.Serialize(tag);

        tag.Reactivate();
        _appointmentTagsRepository.Update(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(TagAgendamento),
            tag.Id,
            AcaoAuditoria.Editar,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: AppointmentTagsAuditSerializer.Serialize(tag),
            cancellationToken: cancellationToken);

        return Result<AppointmentTagCatalogDto>.Success(AppointmentTagsMapper.Map(tag));
    }
}
