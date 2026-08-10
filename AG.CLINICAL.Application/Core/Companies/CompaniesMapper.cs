using System.Text.RegularExpressions;
using AG.CLINICAL.Application.Core.Dtos;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Core.Companies;

internal static class CompaniesMapper
{
    public static CompanyDto Map(Empresa empresa)
    {
        return new CompanyDto(
            empresa.Id,
            empresa.Nome,
            empresa.Cnpj,
            empresa.Telefone,
            empresa.Email,
            empresa.Logo,
            empresa.CorPrincipal,
            empresa.Ativo,
            empresa.CriadoEm,
            empresa.AtualizadoEm);
    }
}

internal static class CompanyValidation
{
    private static readonly Regex HexColorPattern = new(
        "^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$",
        RegexOptions.Compiled);

    public static string? ValidateCorPrincipal(string? corPrincipal)
    {
        if (string.IsNullOrWhiteSpace(corPrincipal))
        {
            return null;
        }

        if (!HexColorPattern.IsMatch(corPrincipal.Trim()))
        {
            return "Informe uma cor principal válida no formato hexadecimal (#RGB ou #RRGGBB).";
        }

        return null;
    }

    public static string? NormalizeCnpj(string? cnpj)
    {
        return string.IsNullOrWhiteSpace(cnpj) ? null : cnpj.Trim();
    }
}
