namespace AG.CLINICAL.Application.Schedules.Dtos;

public sealed record AppointmentTagCatalogDto(
    Guid Id,
    string Nome,
    string Cor,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateAppointmentTagRequest(string Nome, string Cor);

public sealed record UpdateAppointmentTagRequest(string Nome, string Cor);
