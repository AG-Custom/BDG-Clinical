using BGD.CLINICAL.Application.Patients.Dtos;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Patients.Patients;

internal static class PatientsMapper
{
    public static PatientDto Map(Paciente paciente)
    {
        return new PatientDto(
            paciente.Id,
            paciente.UnidadeId,
            paciente.Nome,
            paciente.Cpf,
            paciente.Telefone,
            paciente.Email,
            paciente.DataNascimento,
            paciente.Observacao,
            paciente.Ativo,
            paciente.CriadoEm,
            paciente.AtualizadoEm);
    }

    public static IReadOnlyList<PatientDto> Map(IReadOnlyList<Paciente> pacientes)
    {
        return pacientes.Select(Map).ToList();
    }
}
