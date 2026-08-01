using AG.CLINICAL.Application.Patients.Dtos;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.ValueObjects;

namespace AG.CLINICAL.Application.Patients.Patients;

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
            MapEndereco(paciente.Endereco),
            paciente.Observacao,
            paciente.Ativo,
            paciente.CriadoEm,
            paciente.AtualizadoEm);
    }

    public static IReadOnlyList<PatientDto> Map(IReadOnlyList<Paciente> pacientes)
    {
        return pacientes.Select(Map).ToList();
    }

    private static PatientAddressDto? MapEndereco(Address? endereco)
    {
        if (endereco is null)
        {
            return null;
        }

        return new PatientAddressDto(
            endereco.Cep,
            endereco.Logradouro,
            endereco.Numero,
            endereco.Complemento,
            endereco.Bairro,
            endereco.Cidade,
            endereco.Uf);
    }
}
