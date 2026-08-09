namespace AG.CLINICAL.Application.Schedules.Dtos;

public sealed record UnitOperatingHourDto(
    Guid Id,
    Guid UnidadeId,
    string DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFim,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateUnitOperatingHourRequest(
    string DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFim);

public sealed record UpdateUnitOperatingHourRequest(
    string DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFim,
    bool Ativo = true);

public sealed record SetUnitOperatingHourActiveRequest(bool Ativo);
