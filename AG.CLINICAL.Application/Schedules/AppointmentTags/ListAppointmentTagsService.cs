using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Schedules.Abstractions;
using AG.CLINICAL.Application.Schedules.Dtos;

namespace AG.CLINICAL.Application.Schedules.AppointmentTags;

public interface IListAppointmentTagsService
{
    Task<Result<IReadOnlyList<AppointmentTagCatalogDto>>> ExecuteAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default);
}

public sealed class ListAppointmentTagsService : IListAppointmentTagsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IAppointmentTagsRepository _appointmentTagsRepository;

    public ListAppointmentTagsService(
        ICurrentTenantContext tenantContext,
        IAppointmentTagsRepository appointmentTagsRepository)
    {
        _tenantContext = tenantContext;
        _appointmentTagsRepository = appointmentTagsRepository;
    }

    public async Task<Result<IReadOnlyList<AppointmentTagCatalogDto>>> ExecuteAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var tags = await _appointmentTagsRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            includeInactive,
            cancellationToken);

        return Result<IReadOnlyList<AppointmentTagCatalogDto>>.Success(AppointmentTagsMapper.Map(tags));
    }
}
