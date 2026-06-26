using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

internal sealed record ValidatedCreatePatientApplicationData(
    Guid PacienteId,
    Guid? CompraPacienteId,
    Guid ProdutoId,
    Guid AplicadorId,
    Guid UnidadeId,
    DateTime DataAplicacao,
    decimal QuantidadeUtilizada,
    decimal? Peso,
    string? Observacao,
    IReadOnlyList<Guid> SintomaIds);

internal static class PatientApplicationRequestValidator
{
    public const int DefaultListLimit = 100;
    public const int MaxListLimit = 500;

    public static async Task<Result<ValidatedCreatePatientApplicationData>> ValidateCreateAsync(
        Guid empresaId,
        CreatePatientApplicationRequest request,
        IPatientsRepository patientsRepository,
        IProductsRepository productsRepository,
        IUnitsRepository unitsRepository,
        IEmployeesRepository employeesRepository,
        ISymptomsRepository symptomsRepository,
        IStockBalancesRepository stockBalancesRepository,
        IPatientApplicationsRepository patientApplicationsRepository,
        CancellationToken cancellationToken)
    {
        if (request.PacienteId == Guid.Empty)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("Informe o paciente.");
        }

        if (request.ProdutoId == Guid.Empty)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("Informe o produto.");
        }

        if (request.AplicadorId == Guid.Empty)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("Informe o aplicador.");
        }

        if (request.UnidadeId == Guid.Empty)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("Informe a unidade.");
        }

        if (request.QuantidadeUtilizada <= 0)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("A quantidade utilizada deve ser maior que zero.");
        }

        if (request.Peso.HasValue && request.Peso.Value <= 0)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("O peso deve ser maior que zero quando informado.");
        }

        if (!string.IsNullOrWhiteSpace(request.Observacao) && request.Observacao.Length > 2000)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("A observação deve ter no máximo 2000 caracteres.");
        }

        var paciente = await patientsRepository.GetByIdAndEmpresaIdAsync(request.PacienteId, empresaId, cancellationToken);
        if (paciente is null)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("Paciente não encontrado.");
        }

        if (!paciente.Ativo)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("O paciente está inativo.");
        }

        if (request.CompraPacienteId.HasValue)
        {
            if (request.CompraPacienteId.Value == Guid.Empty)
            {
                return Result<ValidatedCreatePatientApplicationData>.Failure("Compra do paciente inválida.");
            }

            var compraValida = await patientApplicationsRepository.ExistsCompraPacienteForPacienteAsync(
                request.CompraPacienteId.Value,
                request.PacienteId,
                empresaId,
                cancellationToken);

            if (!compraValida)
            {
                return Result<ValidatedCreatePatientApplicationData>.Failure("Compra do paciente não encontrada para este paciente.");
            }
        }

        var produtoExists = await productsRepository.ExistsActiveByIdAndEmpresaIdAsync(
            request.ProdutoId,
            empresaId,
            cancellationToken);

        if (!produtoExists)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("Produto não encontrado ou inativo.");
        }

        var unidade = await unitsRepository.GetByIdAndEmpresaIdAsync(request.UnidadeId, empresaId, cancellationToken);
        if (unidade is null)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("Unidade não encontrada.");
        }

        if (!unidade.Ativo)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("A unidade está inativa.");
        }

        var aplicador = await employeesRepository.GetByIdAndEmpresaIdAsync(request.AplicadorId, empresaId, cancellationToken);
        if (aplicador is null)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("Aplicador não encontrado.");
        }

        if (!aplicador.Ativo)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure("O aplicador está inativo.");
        }

        if (!IsAplicadorForUnidade(aplicador, empresaId, request.UnidadeId))
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure(
                "O funcionário selecionado não é aplicador ativo nesta unidade.");
        }

        var sintomaIds = (request.SintomaIds ?? []).ToList();
        if (sintomaIds.Count > 0)
        {
            var sintomasValidos = await symptomsRepository.AllExistActiveByIdsAsync(
                empresaId,
                sintomaIds,
                cancellationToken);

            if (!sintomasValidos)
            {
                return Result<ValidatedCreatePatientApplicationData>.Failure(
                    "Um ou mais sintomas informados não foram encontrados ou estão inativos.");
            }
        }

        var saldo = await stockBalancesRepository.GetSaldoByUnidadeAndProdutoAsync(
            empresaId,
            request.UnidadeId,
            request.ProdutoId,
            cancellationToken);

        if (saldo < request.QuantidadeUtilizada)
        {
            return Result<ValidatedCreatePatientApplicationData>.Failure(
                "Estoque insuficiente na unidade para a quantidade informada.");
        }

        return Result<ValidatedCreatePatientApplicationData>.Success(new ValidatedCreatePatientApplicationData(
            request.PacienteId,
            request.CompraPacienteId,
            request.ProdutoId,
            request.AplicadorId,
            request.UnidadeId,
            request.DataAplicacao,
            request.QuantidadeUtilizada,
            request.Peso,
            string.IsNullOrWhiteSpace(request.Observacao) ? null : request.Observacao.Trim(),
            sintomaIds));
    }

    public static async Task<Result<IReadOnlyList<Guid>>> ValidateUpdateAsync(
        Guid empresaId,
        UpdatePatientApplicationRequest request,
        ISymptomsRepository symptomsRepository,
        CancellationToken cancellationToken)
    {
        if (request.Peso.HasValue && request.Peso.Value <= 0)
        {
            return Result<IReadOnlyList<Guid>>.Failure("O peso deve ser maior que zero quando informado.");
        }

        if (!string.IsNullOrWhiteSpace(request.Observacao) && request.Observacao.Length > 2000)
        {
            return Result<IReadOnlyList<Guid>>.Failure("A observação deve ter no máximo 2000 caracteres.");
        }

        var sintomaIds = (request.SintomaIds ?? []).ToList();
        if (sintomaIds.Count > 0)
        {
            var sintomasValidos = await symptomsRepository.AllExistActiveByIdsAsync(
                empresaId,
                sintomaIds,
                cancellationToken);

            if (!sintomasValidos)
            {
                return Result<IReadOnlyList<Guid>>.Failure(
                    "Um ou mais sintomas informados não foram encontrados ou estão inativos.");
            }
        }

        return Result<IReadOnlyList<Guid>>.Success(sintomaIds);
    }

    public static Result<int> ValidateListLimit(int? limit)
    {
        if (!limit.HasValue)
        {
            return Result<int>.Success(DefaultListLimit);
        }

        if (limit.Value <= 0)
        {
            return Result<int>.Failure("O limite deve ser maior que zero.");
        }

        if (limit.Value > MaxListLimit)
        {
            return Result<int>.Failure($"O limite máximo é {MaxListLimit}.");
        }

        return Result<int>.Success(limit.Value);
    }

    private static bool IsAplicadorForUnidade(Funcionario funcionario, Guid empresaId, Guid unidadeId)
    {
        return funcionario.Vinculos.Any(vinculo =>
            vinculo.Ativo
            && vinculo.FlagAplicador
            && (
                (vinculo.EmpresaId.HasValue && vinculo.EmpresaId.Value == empresaId)
                || (vinculo.UnidadeId.HasValue && vinculo.UnidadeId.Value == unidadeId)));
    }
}
