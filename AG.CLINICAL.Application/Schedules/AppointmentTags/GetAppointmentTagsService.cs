using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Dtos;

namespace AG.CLINICAL.Application.Schedules.AppointmentTags;

public interface IGetAppointmentTagsService
{
    Task<Result<AppointmentTagCatalogDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}

public sealed class GetAppointmentTagsService : IGetAppointmentTagsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAppointmentTagsRepository _appointmentTagsRepository;

    public GetAppointmentTagsService(
        ICurrentTenantContext tenantContext,
        IAppointmentTagsRepository appointmentTagsRepository)
    {
        _tenantContext = tenantContext;
        _appointmentTagsRepository = appointmentTagsRepository;
    }

    public async Task<Result<AppointmentTagCatalogDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var tag = await _appointmentTagsRepository.GetByIdAndEmpresaIdAsync(
            id,
            _tenantContext.EmpresaId,
            cancellationToken);

        if (tag is null)
        {
            return Result<AppointmentTagCatalogDto>.Failure("Tag não encontrada.");
        }

        return Result<AppointmentTagCatalogDto>.Success(AppointmentTagsMapper.Map(tag));
    }
}
