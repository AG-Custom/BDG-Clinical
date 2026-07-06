using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Patients.Dtos;

namespace BGD.CLINICAL.Application.Patients.Patients;

internal static class PatientRequestValidator
{
    public static async Task<Result<ValidatedPatientData>> ValidateAsync(
        Guid empresaId,
        Guid? unidadeId,
        IReadOnlyList<Guid>? unidadeIds,
        string nome,
        string? cpf,
        string? telefone,
        string? email,
        string? observacao,
        Guid? excludePatientId,
        IPatientsRepository patientsRepository,
        CancellationToken cancellationToken)
    {
        var resolvedUnidadeIdsResult = ResolveUnidadeIds(unidadeId, unidadeIds);
        if (resolvedUnidadeIdsResult.IsFailure)
        {
            return Result<ValidatedPatientData>.Failure(resolvedUnidadeIdsResult.Error!);
        }

        var resolvedUnidadeIds = resolvedUnidadeIdsResult.Value!;

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

        if (!await patientsRepository.AllActiveUnitsExistInEmpresaAsync(
                empresaId,
                resolvedUnidadeIds,
                cancellationToken))
        {
            return Result<ValidatedPatientData>.Failure("Uma ou mais unidades não foram encontradas ou estão inativas.");
        }

        if (normalizedCpf is not null
            && await patientsRepository.ExistsByCpfAsync(empresaId, normalizedCpf, excludePatientId, cancellationToken))
        {
            return Result<ValidatedPatientData>.Failure("Já existe um paciente com este CPF nesta empresa.");
        }

        return Result<ValidatedPatientData>.Success(new ValidatedPatientData(
            resolvedUnidadeIds,
            nome.Trim(),
            normalizedCpf,
            PatientValidation.NormalizeTelefone(telefone),
            normalizedEmail,
            PatientValidation.NormalizeObservacao(observacao)));
    }

    internal static Result<IReadOnlyList<Guid>> ResolveUnidadeIds(
        Guid? unidadeId,
        IReadOnlyList<Guid>? unidadeIds)
    {
        IReadOnlyList<Guid> resolved;

        if (unidadeIds is { Count: > 0 })
        {
            resolved = unidadeIds
                .Where(id => id != Guid.Empty)
                .ToList();
        }
        else if (unidadeId.HasValue && unidadeId.Value != Guid.Empty)
        {
            resolved = [unidadeId.Value];
        }
        else
        {
            resolved = [];
        }

        if (resolved.Count == 0)
        {
            return Result<IReadOnlyList<Guid>>.Failure("Informe ao menos uma unidade do paciente.");
        }

        if (resolved.Count != resolved.Distinct().Count())
        {
            return Result<IReadOnlyList<Guid>>.Failure(
                "Não é permitido repetir a mesma unidade no paciente.");
        }

        return Result<IReadOnlyList<Guid>>.Success(resolved.Distinct().ToList());
    }
}

internal sealed record ValidatedPatientData(
    IReadOnlyList<Guid> UnidadeIds,
    string Nome,
    string? Cpf,
    string? Telefone,
    string? Email,
    string? Observacao);
