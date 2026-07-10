using BGD.CLINICAL.Application.Patients.Dtos;
using BGD.CLINICAL.Domain.ValueObjects;

namespace BGD.CLINICAL.Application.Patients.Patients;

internal static class PatientValidation
{
    public static string? NormalizeCpf(string? cpf) => Cpf.Normalize(cpf);

    public static string? ValidateCpf(string? cpf) => Cpf.Validate(cpf);

    public static string? NormalizeTelefone(string? telefone)
    {
        return string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
    }

    public static string? NormalizeEmail(string? email) => Email.Normalize(email);

    public static Address? TryCreateEndereco(PatientAddressRequest? request, out string? error)
    {
        if (request is null)
        {
            error = null;
            return null;
        }

        return Address.TryCreate(
            request.Cep,
            request.Logradouro,
            request.Numero,
            request.Complemento,
            request.Bairro,
            request.Cidade,
            request.Uf,
            out error);
    }

    public static string? NormalizeObservacao(string? observacao)
    {
        return string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
    }
}
