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
            paciente.Nome,
            paciente.Cpf,
            paciente.Telefone,
            paciente.Email,
            paciente.DataNascimento,
            paciente.Observacao,
            paciente.Ativo,
        }, Options);
    }
}
