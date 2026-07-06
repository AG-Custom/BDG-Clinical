using BGD.CLINICAL.Application.Patients.Dtos;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Patients.Patients;

internal static class PatientsMapper
{
    public static PatientDto Map(Paciente paciente)
    {
        var unidades = paciente.UnidadesVinculadas
            .OrderBy(item => item.CriadoEm)
            .Select(item => new PatientUnitDto(
                item.UnidadeId,
                item.Unidade?.Nome ?? string.Empty))
            .ToList();

        if (unidades.Count == 0 && paciente.UnidadeId != Guid.Empty)
        {
            unidades.Add(new PatientUnitDto(
                paciente.UnidadeId,
                paciente.Unidade?.Nome ?? string.Empty));
        }

        var unidadeIds = unidades.Select(unidade => unidade.Id).ToList();
        var primeiraUnidade = unidades.FirstOrDefault();

        return new PatientDto(
            paciente.Id,
            primeiraUnidade?.Id ?? paciente.UnidadeId,
            unidades,
            unidadeIds,
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
