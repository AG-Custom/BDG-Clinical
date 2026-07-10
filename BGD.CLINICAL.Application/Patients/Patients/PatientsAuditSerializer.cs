using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Patients.Patients;

internal static class PatientsAuditSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(Paciente paciente)
    {
        return JsonSerializer.Serialize(new
        {
            paciente.Id,
            paciente.EmpresaId,
            paciente.UnidadeId,
            UnidadeIds = paciente.GetUnidadeIds(),
            paciente.Nome,
            paciente.Cpf,
            paciente.Telefone,
            paciente.Email,
            paciente.DataNascimento,
            Endereco = paciente.Endereco is null
                ? null
                : new
                {
                    paciente.Endereco.Cep,
                    paciente.Endereco.Logradouro,
                    paciente.Endereco.Numero,
                    paciente.Endereco.Complemento,
                    paciente.Endereco.Bairro,
                    paciente.Endereco.Cidade,
                    paciente.Endereco.Uf,
                },
            paciente.Observacao,
            paciente.Ativo,
        }, Options);
    }
}
