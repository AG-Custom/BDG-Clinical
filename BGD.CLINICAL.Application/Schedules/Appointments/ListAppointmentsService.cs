using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Application.Schedules.Abstractions;
using BGD.CLINICAL.Application.Schedules.Dtos;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Schedules.Appointments;

public interface IListAppointmentsService
{
    Task<Result<IReadOnlyList<AppointmentDto>>> ExecuteAsync(
        Guid? unidadeId,
        Guid? funcionarioId,
        Guid? pacienteId,
        string? status,
        DateTime? dataInicioFrom,
        DateTime? dataInicioTo,
        CancellationToken cancellationToken = default);
}

public sealed class ListAppointmentsService : IListAppointmentsService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUsersRepository _usersRepository;
    private readonly IPermissionChecker _permissionChecker;
    private readonly IAppointmentsRepository _appointmentsRepository;

    public ListAppointmentsService(
        ICurrentTenantContext tenantContext,
        IUsersRepository usersRepository,
        IPermissionChecker permissionChecker,
        IAppointmentsRepository appointmentsRepository)
    {
        _tenantContext = tenantContext;
        _usersRepository = usersRepository;
        _permissionChecker = permissionChecker;
        _appointmentsRepository = appointmentsRepository;
    }

    public async Task<Result<IReadOnlyList<AppointmentDto>>> ExecuteAsync(
        Guid? unidadeId,
        Guid? funcionarioId,
        Guid? pacienteId,
        string? status,
        DateTime? dataInicioFrom,
        DateTime? dataInicioTo,
        CancellationToken cancellationToken = default)
    {
        var scopeFuncionarioId = await AppointmentScopeResolver.ResolveFuncionarioFilterAsync(
            _tenantContext,
            _usersRepository,
            _permissionChecker,
            cancellationToken);

        if (scopeFuncionarioId == Guid.Empty)
        {
            return Result<IReadOnlyList<AppointmentDto>>.Failure("Usuário sem permissão para visualizar a agenda.");
        }

        StatusAgendamento? statusFilter = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<StatusAgendamento>(status, ignoreCase: true, out var parsed))
            {
                return Result<IReadOnlyList<AppointmentDto>>.Failure("Status de agendamento inválido.");
            }

            statusFilter = parsed;
        }

        var effectiveFuncionarioId = scopeFuncionarioId ?? funcionarioId;

        var agendamentos = await _appointmentsRepository.ListByEmpresaIdAsync(
            _tenantContext.EmpresaId,
            unidadeId,
            effectiveFuncionarioId,
            pacienteId,
            statusFilter,
            dataInicioFrom,
            dataInicioTo,
            cancellationToken);

        if (scopeFuncionarioId.HasValue)
        {
            agendamentos = agendamentos
                .Where(agendamento => agendamento.FuncionarioId == scopeFuncionarioId.Value)
                .ToList();
        }

        var dtos = agendamentos.Select(AppointmentsMapper.Map).ToList();
        return Result<IReadOnlyList<AppointmentDto>>.Success(dtos);
    }
}
