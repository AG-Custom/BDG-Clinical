using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Patients.Dtos;

namespace BGD.CLINICAL.Application.Patients.Patients;

internal static class PatientRequestValidator
{
    public static async Task<Result<ValidatedPatientData>> ValidateAsync(
        Guid empresaId,
        Guid unidadeId,
        string nome,
        string? cpf,
        string? telefone,
        string? email,
        string? observacao,
        Guid? excludePatientId,
        IPatientsRepository patientsRepository,
        CancellationToken cancellationToken)
    {
        if (unidadeId == Guid.Empty)
        {
            return Result<ValidatedPatientData>.Failure("Informe a unidade do paciente.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            return Result<ValidatedPatientData>.Failure("Informe o nome do paciente.");
        }

        var cpfError = PatientValidation.ValidateCpf(cpf);
        if (cpfError is not null)
        {
            return Result<ValidatedPatientData>.Failure(cpfError);
        }

        var normalizedCpf = PatientValidation.NormalizeCpf(cpf);
        var normalizedEmail = PatientValidation.NormalizeEmail(email);

        if (normalizedEmail is not null && !IdentityValidation.IsValidEmail(normalizedEmail))
        {
            return Result<ValidatedPatientData>.Failure("Informe um e-mail válido.");
        }

        if (!await patientsRepository.ExistsActiveUnidadeInEmpresaAsync(unidadeId, empresaId, cancellationToken))
        {
            return Result<ValidatedPatientData>.Failure("Unidade não encontrada ou inativa.");
        }

        if (normalizedCpf is not null
            && await patientsRepository.ExistsByCpfAsync(empresaId, normalizedCpf, excludePatientId, cancellationToken))
        {
            return Result<ValidatedPatientData>.Failure("Já existe um paciente com este CPF nesta empresa.");
        }

        return Result<ValidatedPatientData>.Success(new ValidatedPatientData(
            unidadeId,
            nome.Trim(),
            normalizedCpf,
            PatientValidation.NormalizeTelefone(telefone),
            normalizedEmail,
            PatientValidation.NormalizeObservacao(observacao)));
    }
}

internal sealed record ValidatedPatientData(
    Guid UnidadeId,
    string Nome,
    string? Cpf,
    string? Telefone,
    string? Email,
    string? Observacao);
