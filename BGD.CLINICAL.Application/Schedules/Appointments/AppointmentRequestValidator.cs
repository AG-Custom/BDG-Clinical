using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Schedules.Abstractions;
using BGD.CLINICAL.Application.Schedules.Dtos;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Schedules.Appointments;

internal sealed record ValidatedAppointmentData(
    Guid UnidadeId,
    Guid PacienteId,
    Guid FuncionarioId,
    IReadOnlyList<Guid> ProcedimentoIds,
    Guid? CompraPacienteId,
    TipoAgendamento Tipo,
    DateTime DataInicio,
    DateTime DataFim,
    string? Observacao,
    bool ExcecaoHorario);

internal static class AppointmentRequestValidator
{
    public static async Task<Result<ValidatedAppointmentData>> ValidateAsync(
        Guid empresaId,
        CreateAppointmentRequest request,
        Guid? excludeAppointmentId,
        IAppointmentsRepository appointmentsRepository,
        IPatientsRepository patientsRepository,
        IEmployeesRepository employeesRepository,
        IUnitsRepository unitsRepository,
        IProceduresRepository proceduresRepository,
        IUnitOperatingHoursRepository operatingHoursRepository,
        CancellationToken cancellationToken)
    {
        var procedimentoIdsResult = ResolveProcedimentoIds(request.ProcedimentoId, request.ProcedimentoIds);
        if (procedimentoIdsResult.IsFailure)
        {
            return Result<ValidatedAppointmentData>.Failure(procedimentoIdsResult.Error!);
        }

        return await ValidateCoreAsync(
            empresaId,
            request.UnidadeId,
            request.PacienteId,
            request.FuncionarioId,
            request.Tipo,
            request.DataInicio,
            request.DataFim,
            procedimentoIdsResult.Value!,
            request.CompraPacienteId,
            request.Observacao,
            request.ExcecaoHorario != 0,
            excludeAppointmentId,
            appointmentsRepository,
            patientsRepository,
            employeesRepository,
            unitsRepository,
            proceduresRepository,
            operatingHoursRepository,
            cancellationToken);
    }

    public static Task<Result<ValidatedAppointmentData>> ValidateAsync(
        Guid empresaId,
        Guid appointmentId,
        UpdateAppointmentRequest request,
        IAppointmentsRepository appointmentsRepository,
        IPatientsRepository patientsRepository,
        IEmployeesRepository employeesRepository,
        IUnitsRepository unitsRepository,
        IProceduresRepository proceduresRepository,
        IUnitOperatingHoursRepository operatingHoursRepository,
        CancellationToken cancellationToken)
    {
        var procedimentoIdsResult = ResolveProcedimentoIds(request.ProcedimentoId, request.ProcedimentoIds);
        if (procedimentoIdsResult.IsFailure)
        {
            return Task.FromResult(Result<ValidatedAppointmentData>.Failure(procedimentoIdsResult.Error!));
        }

        return ValidateCoreAsync(
            empresaId,
            request.UnidadeId,
            request.PacienteId,
            request.FuncionarioId,
            request.Tipo,
            request.DataInicio,
            request.DataFim,
            procedimentoIdsResult.Value!,
            request.CompraPacienteId,
            request.Observacao,
            request.ExcecaoHorario != 0,
            appointmentId,
            appointmentsRepository,
            patientsRepository,
            employeesRepository,
            unitsRepository,
            proceduresRepository,
            operatingHoursRepository,
            cancellationToken);
    }

    internal static Result<IReadOnlyList<Guid>> ResolveProcedimentoIds(
        Guid? procedimentoId,
        IReadOnlyList<Guid>? procedimentoIds)
    {
        IReadOnlyList<Guid> resolved;

        if (procedimentoIds is { Count: > 0 })
        {
            resolved = procedimentoIds
                .Where(id => id != Guid.Empty)
                .ToList();
        }
        else if (procedimentoId.HasValue && procedimentoId.Value != Guid.Empty)
        {
            resolved = [procedimentoId.Value];
        }
        else
        {
            resolved = [];
        }

        if (resolved.Count != resolved.Distinct().Count())
        {
            return Result<IReadOnlyList<Guid>>.Failure(
                "Não é permitido repetir o mesmo procedimento no agendamento.");
        }

        return Result<IReadOnlyList<Guid>>.Success(resolved.Distinct().ToList());
    }

    private static async Task<Result<ValidatedAppointmentData>> ValidateCoreAsync(
        Guid empresaId,
        Guid unidadeId,
        Guid pacienteId,
        Guid funcionarioId,
        string tipo,
        DateTime dataInicio,
        DateTime dataFim,
        IReadOnlyList<Guid> procedimentoIds,
        Guid? compraPacienteId,
        string? observacao,
        bool excecaoHorario,
        Guid? excludeAppointmentId,
        IAppointmentsRepository appointmentsRepository,
        IPatientsRepository patientsRepository,
        IEmployeesRepository employeesRepository,
        IUnitsRepository unitsRepository,
        IProceduresRepository proceduresRepository,
        IUnitOperatingHoursRepository operatingHoursRepository,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TipoAgendamento>(tipo, ignoreCase: true, out var tipoAgendamento))
        {
            return Result<ValidatedAppointmentData>.Failure("Tipo de agendamento inválido.");
        }

        if (unidadeId == Guid.Empty)
        {
            return Result<ValidatedAppointmentData>.Failure("Informe a unidade do agendamento.");
        }

        if (pacienteId == Guid.Empty)
        {
            return Result<ValidatedAppointmentData>.Failure("Informe o paciente do agendamento.");
        }

        if (funcionarioId == Guid.Empty)
        {
            return Result<ValidatedAppointmentData>.Failure("Informe o funcionário do agendamento.");
        }

        if (dataFim <= dataInicio)
        {
            return Result<ValidatedAppointmentData>.Failure("A data de término deve ser posterior à data de início.");
        }

        if (tipoAgendamento == TipoAgendamento.Aplicacao && procedimentoIds.Count == 0)
        {
            return Result<ValidatedAppointmentData>.Failure("Informe ao menos um procedimento para agendamentos do tipo aplicação.");
        }

        if (tipoAgendamento == TipoAgendamento.Aplicacao
            && (!compraPacienteId.HasValue || compraPacienteId.Value == Guid.Empty))
        {
            return Result<ValidatedAppointmentData>.Failure("Informe a compra de pacote para agendamentos do tipo aplicação.");
        }

        if (tipoAgendamento != TipoAgendamento.Aplicacao && procedimentoIds.Count > 0)
        {
            return Result<ValidatedAppointmentData>.Failure("Procedimento só pode ser informado em agendamentos do tipo aplicação.");
        }

        if (tipoAgendamento != TipoAgendamento.Aplicacao
            && compraPacienteId.HasValue
            && compraPacienteId.Value != Guid.Empty)
        {
            return Result<ValidatedAppointmentData>.Failure("Compra de pacote só pode ser informada em agendamentos do tipo aplicação.");
        }

        var resolvedCompraId = tipoAgendamento == TipoAgendamento.Aplicacao
            ? compraPacienteId
            : null;

        var unidade = await unitsRepository.GetByIdAndEmpresaIdAsync(unidadeId, empresaId, cancellationToken);
        if (unidade is null || !unidade.Ativo)
        {
            return Result<ValidatedAppointmentData>.Failure("Unidade não encontrada ou inativa.");
        }

        var paciente = await patientsRepository.GetByIdAndEmpresaIdWithDetailsAsync(pacienteId, empresaId, cancellationToken);
        if (paciente is null || !paciente.Ativo)
        {
            return Result<ValidatedAppointmentData>.Failure("Paciente não encontrado ou inativo.");
        }

        if (!paciente.IsLinkedToUnidade(unidadeId))
        {
            return Result<ValidatedAppointmentData>.Failure("O paciente não pertence à unidade informada.");
        }

        var funcionario = await employeesRepository.GetByIdAndEmpresaIdAsync(funcionarioId, empresaId, cancellationToken);
        if (funcionario is null || !funcionario.Ativo)
        {
            return Result<ValidatedAppointmentData>.Failure("Funcionário não encontrado ou inativo.");
        }

        foreach (var procedimentoId in procedimentoIds)
        {
            if (!await proceduresRepository.ExistsActiveByIdAndEmpresaIdAsync(procedimentoId, empresaId, cancellationToken))
            {
                return Result<ValidatedAppointmentData>.Failure("Procedimento não encontrado ou inativo.");
            }
        }

        if (await appointmentsRepository.HasScheduleBlockAsync(
                empresaId,
                funcionarioId,
                unidadeId,
                dataInicio,
                dataFim,
                cancellationToken))
        {
            return Result<ValidatedAppointmentData>.Failure("O horário informado está bloqueado na agenda do funcionário.");
        }

        if (await appointmentsRepository.HasActiveAvailabilityConfiguredAsync(
                empresaId,
                funcionarioId,
                unidadeId,
                cancellationToken))
        {
            if (!await appointmentsRepository.HasMatchingAvailabilityAsync(
                    empresaId,
                    funcionarioId,
                    unidadeId,
                    dataInicio,
                    dataFim,
                    cancellationToken))
            {
                return Result<ValidatedAppointmentData>.Failure("O horário informado está fora da disponibilidade do funcionário.");
            }
        }

        if (await operatingHoursRepository.HasActiveOperatingHoursConfiguredAsync(
                empresaId,
                unidadeId,
                cancellationToken))
        {
            var dentroDoFuncionamento = await operatingHoursRepository.HasMatchingOperatingHoursAsync(
                empresaId,
                unidadeId,
                dataInicio,
                dataFim,
                cancellationToken);

            if (!dentroDoFuncionamento && !excecaoHorario)
            {
                return Result<ValidatedAppointmentData>.Failure(
                    "O horário informado está fora do funcionamento da unidade. Envie excecaoHorario = 1 para confirmar o agendamento.");
            }
        }

        return Result<ValidatedAppointmentData>.Success(new ValidatedAppointmentData(
            unidadeId,
            pacienteId,
            funcionarioId,
            procedimentoIds,
            resolvedCompraId,
            tipoAgendamento,
            dataInicio,
            dataFim,
            string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim(),
            excecaoHorario));
    }
}
