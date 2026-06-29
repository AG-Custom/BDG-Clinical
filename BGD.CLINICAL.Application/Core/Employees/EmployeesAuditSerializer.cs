using System.Text.Json;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Core.Employees;

internal static class EmployeesAuditSerializer
{
    public static string Serialize(Funcionario funcionario, Guid empresaId, string? emailLogin)
    {
        var payload = new
        {
            funcionario.Id,
            funcionario.Nome,
            funcionario.Telefone,
            funcionario.Email,
            EmailLogin = emailLogin,
            funcionario.Ativo,
            Links = funcionario.Vinculos
                .Where(vinculo => vinculo.BelongsToEmpresa(empresaId))
                .Select(vinculo => new
                {
                    vinculo.Id,
                    vinculo.EmpresaId,
                    vinculo.UnidadeId,
                    vinculo.CargoId,
                    vinculo.Ativo
                })
        };

        return JsonSerializer.Serialize(payload);
    }
}
