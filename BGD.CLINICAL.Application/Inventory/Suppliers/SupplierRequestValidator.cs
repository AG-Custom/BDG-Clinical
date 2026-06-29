using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.Dtos;

namespace BGD.CLINICAL.Application.Inventory.Suppliers;

internal sealed record ValidatedSupplierData(
    string Nome,
    string Cnpj,
    string? Telefone,
    string? Email,
    string? Observacao);

internal static class SupplierRequestValidator
{
    public static async Task<Result<ValidatedSupplierData>> ValidateAsync(
        Guid empresaId,
        string? nome,
        string? cnpj,
        string? telefone,
        string? email,
        string? observacao,
        Guid? excludeSupplierId,
        ISuppliersRepository suppliersRepository,
        CancellationToken cancellationToken)
    {
        var normalizedNome = SupplierValidation.NormalizeNome(nome);
        if (normalizedNome is null)
        {
            return Result<ValidatedSupplierData>.Failure("Informe o nome do fornecedor.");
        }

        var cnpjError = SupplierValidation.ValidateCnpj(cnpj);
        if (cnpjError is not null)
        {
            return Result<ValidatedSupplierData>.Failure(cnpjError);
        }

        var normalizedCnpj = SupplierValidation.NormalizeCnpj(cnpj)!;

        if (await suppliersRepository.ExistsByNomeAsync(empresaId, normalizedNome, excludeSupplierId, cancellationToken))
        {
            return Result<ValidatedSupplierData>.Failure("Já existe um fornecedor com este nome.");
        }

        if (await suppliersRepository.ExistsByCnpjAsync(empresaId, normalizedCnpj, excludeSupplierId, cancellationToken))
        {
            return Result<ValidatedSupplierData>.Failure("Já existe um fornecedor com este CNPJ.");
        }

        var observacaoError = SupplierValidation.ValidateObservacao(observacao);
        if (observacaoError is not null)
        {
            return Result<ValidatedSupplierData>.Failure(observacaoError);
        }

        return Result<ValidatedSupplierData>.Success(new ValidatedSupplierData(
            normalizedNome,
            normalizedCnpj,
            SupplierValidation.NormalizeTelefone(telefone),
            SupplierValidation.NormalizeEmail(email),
            SupplierValidation.NormalizeObservacao(observacao)));
    }
}
