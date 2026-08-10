using BGD.CLINICAL.Application.Applications.Abstractions;
using BGD.CLINICAL.Application.Applications.Dtos;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Application.Inventory.StockMovements;
using BGD.CLINICAL.Application.Patients.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Applications.PatientApplications;

internal sealed record ValidatedCreatePatientApplicationProcedureData(
    Guid? ProdutoId,
    Guid ProcedimentoId,
    decimal? QuantidadeUtilizada,
    Guid? LoteProdutoId,
    bool ConsumirInsumosKit,
    IReadOnlyList<StockConsumptionLine> StockLines);

internal sealed record ValidatedCreatePatientApplicationsData(
    Guid PacienteId,
    Guid? CompraPacienteId,
    Guid AplicadorId,
    Guid UnidadeId,
    DateTime DataAplicacao,
    decimal? Peso,
    string? Observacao,
    IReadOnlyList<Guid> SintomaIds,
    IReadOnlyList<ValidatedCreatePatientApplicationProcedureData> Procedimentos);

internal static class PatientApplicationRequestValidator
{
    public const int DefaultListLimit = 100;
    public const int MaxListLimit = 500;

    public static async Task<Result<ValidatedCreatePatientApplicationsData>> ValidateCreateAsync(
        Guid empresaId,
        CreatePatientApplicationRequest request,
        IPatientsRepository patientsRepository,
        IProductsRepository productsRepository,
        IProceduresRepository proceduresRepository,
        IUnitsRepository unitsRepository,
        IEmployeesRepository employeesRepository,
        ISymptomsRepository symptomsRepository,
        IStockBalancesRepository stockBalancesRepository,
        BGD.CLINICAL.Application.Packages.Abstractions.IPatientPurchasesRepository patientPurchasesRepository,
        IMedicationLotStockService medicationLotStockService,
        CancellationToken cancellationToken)
    {
        if (request.PacienteId == Guid.Empty)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure("Informe o paciente.");
        }

        if (request.AplicadorId == Guid.Empty)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure("Informe o aplicador.");
        }

        if (request.UnidadeId == Guid.Empty)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure("Informe a unidade.");
        }

        var procedimentosResolvidos = PatientApplicationProcedureResolver.Resolve(
            request.ProcedimentoId,
            request.QuantidadeUtilizada,
            request.LoteProdutoId,
            request.ConsumirInsumosKit,
            request.InsumosManuais,
            request.Procedimentos);

        if (procedimentosResolvidos.IsFailure)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure(procedimentosResolvidos.Error!);
        }

        if (request.Peso.HasValue && request.Peso.Value <= 0)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure("O peso deve ser maior que zero quando informado.");
        }

        if (!string.IsNullOrWhiteSpace(request.Observacao) && request.Observacao.Length > 2000)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure("A observação deve ter no máximo 2000 caracteres.");
        }

        var paciente = await patientsRepository.GetByIdAndEmpresaIdWithDetailsAsync(request.PacienteId, empresaId, cancellationToken);
        if (paciente is null)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure("Paciente não encontrado.");
        }

        if (!paciente.Ativo)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure("O paciente está inativo.");
        }

        if (!paciente.IsLinkedToUnidade(request.UnidadeId))
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure(
                "O paciente não pertence à unidade informada.");
        }

        var unidade = await unitsRepository.GetByIdAndEmpresaIdAsync(request.UnidadeId, empresaId, cancellationToken);
        if (unidade is null)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure("Unidade não encontrada.");
        }

        if (!unidade.Ativo)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure("A unidade está inativa.");
        }

        CompraPaciente? compra = null;
        Guid? compraPacienteId = null;

        if (request.CompraPacienteId.HasValue && request.CompraPacienteId.Value != Guid.Empty)
        {
            compraPacienteId = request.CompraPacienteId.Value;
            compra = await patientPurchasesRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                compraPacienteId.Value,
                empresaId,
                cancellationToken);

            if (compra is null)
            {
                return Result<ValidatedCreatePatientApplicationsData>.Failure("Compra de pacote não encontrada.");
            }

            try
            {
                compra.EnsurePodeAplicar(request.PacienteId, null, null);
            }
            catch (DomainException exception)
            {
                return Result<ValidatedCreatePatientApplicationsData>.Failure(exception.Message);
            }
        }

        var aplicador = await employeesRepository.GetByIdAndEmpresaIdAsync(request.AplicadorId, empresaId, cancellationToken);
        if (aplicador is null)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure("Aplicador não encontrado.");
        }

        if (!aplicador.Ativo)
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure("O aplicador está inativo.");
        }

        if (!IsAplicadorForUnidade(aplicador, empresaId, request.UnidadeId))
        {
            return Result<ValidatedCreatePatientApplicationsData>.Failure(
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
                return Result<ValidatedCreatePatientApplicationsData>.Failure(
                    "Um ou mais sintomas informados não foram encontrados ou estão inativos.");
            }
        }

        var procedimentosValidados = new List<ValidatedCreatePatientApplicationProcedureData>();

        foreach (var item in procedimentosResolvidos.Value!)
        {
            var procedimento = await proceduresRepository.GetByIdAndEmpresaIdWithDetailsAsync(
                item.ProcedimentoId,
                empresaId,
                cancellationToken);

            if (procedimento is null || !procedimento.Ativo)
            {
                return Result<ValidatedCreatePatientApplicationsData>.Failure("Procedimento não encontrado ou inativo.");
            }

            var produtoIdResolvido = procedimento.ProdutoAplicadoId;
            var quantidade = item.QuantidadeUtilizada;

            if (procedimento.ProdutoAplicadoId.HasValue)
            {
                if (!quantidade.HasValue || quantidade.Value <= 0)
                {
                    return Result<ValidatedCreatePatientApplicationsData>.Failure("A quantidade utilizada deve ser maior que zero.");
                }
            }
            else if (quantidade.HasValue)
            {
                return Result<ValidatedCreatePatientApplicationsData>.Failure(
                    "Quantidade utilizada não se aplica a procedimentos sem produto aplicado.");
            }

            if (compra is not null)
            {
                try
                {
                    compra.EnsurePodeAplicar(
                        request.PacienteId,
                        produtoIdResolvido,
                        quantidade);
                }
                catch (DomainException exception)
                {
                    return Result<ValidatedCreatePatientApplicationsData>.Failure(exception.Message);
                }
            }

            var productIds = new HashSet<Guid>();
            if (produtoIdResolvido.HasValue)
            {
                productIds.Add(produtoIdResolvido.Value);
            }

            if (item.ConsumirInsumosKit)
            {
                foreach (var kitItem in procedimento.Itens)
                {
                    productIds.Add(kitItem.ProdutoId);
                }
            }
            else
            {
                foreach (var manual in item.InsumosManuais ?? [])
                {
                    if (manual.ProdutoId != Guid.Empty)
                    {
                        productIds.Add(manual.ProdutoId);
                    }
                }
            }

            var produtos = await productsRepository.GetActiveByIdsAndEmpresaIdAsync(
                empresaId,
                productIds,
                cancellationToken);

            if (produtos.Count != productIds.Count)
            {
                return Result<ValidatedCreatePatientApplicationsData>.Failure(
                    "Um ou mais produtos do consumo não foram encontrados ou estão inativos.");
            }

            var productsById = produtos.ToDictionary(produto => produto.Id);

            var insumosManuaisValidados = PatientApplicationManualSuppliesValidator.NormalizeAndValidate(
                item.ConsumirInsumosKit,
                item.InsumosManuais,
                produtoIdResolvido,
                productsById);

            if (insumosManuaisValidados.IsFailure)
            {
                return Result<ValidatedCreatePatientApplicationsData>.Failure(insumosManuaisValidados.Error!);
            }

            var stockLines = PatientApplicationStockPlanner.BuildLines(
                quantidade,
                procedimento,
                productsById,
                item.ConsumirInsumosKit,
                insumosManuaisValidados.Value);

            Guid? loteProdutoId = item.LoteProdutoId;
            Produto? produtoAplicado = null;

            if (produtoIdResolvido.HasValue
                && productsById.TryGetValue(produtoIdResolvido.Value, out var produtoResolvido))
            {
                produtoAplicado = produtoResolvido;
            }

            // Lote opcional temporariamente (pós-migração). Quando informado, valida alocação;
            // sem lote, segue baixa de estoque sem vínculo de lote.
            if (produtoAplicado is not null && medicationLotStockService.RequiresLot(produtoAplicado))
            {
                if (loteProdutoId.HasValue && loteProdutoId.Value != Guid.Empty)
                {
                    try
                    {
                        await medicationLotStockService.AllocateFromLotAsync(
                            empresaId,
                            request.UnidadeId,
                            produtoAplicado,
                            loteProdutoId.Value,
                            quantidade!.Value,
                            cancellationToken);
                    }
                    catch (DomainException exception)
                    {
                        return Result<ValidatedCreatePatientApplicationsData>.Failure(exception.Message);
                    }
                }
                else
                {
                    loteProdutoId = null;
                }
            }
            else if (loteProdutoId.HasValue && loteProdutoId.Value != Guid.Empty)
            {
                return Result<ValidatedCreatePatientApplicationsData>.Failure(
                    "Lote só pode ser informado para procedimentos com medicamento que controla lote.");
            }
            else
            {
                loteProdutoId = null;
            }

            foreach (var line in stockLines.Where(line => line.ControlaEstoque))
            {
                var saldo = await stockBalancesRepository.GetSaldoByUnidadeAndProdutoAsync(
                    empresaId,
                    request.UnidadeId,
                    line.ProdutoId,
                    cancellationToken);

                if (saldo < line.Quantidade)
                {
                    return Result<ValidatedCreatePatientApplicationsData>.Failure(
                        $"Estoque insuficiente para \"{line.ProdutoNome}\" na unidade selecionada. Saldo: {saldo} | Necessário: {line.Quantidade}");
                }
            }

            procedimentosValidados.Add(new ValidatedCreatePatientApplicationProcedureData(
                produtoIdResolvido,
                procedimento.Id,
                quantidade,
                loteProdutoId,
                item.ConsumirInsumosKit,
                stockLines));
        }

        return Result<ValidatedCreatePatientApplicationsData>.Success(new ValidatedCreatePatientApplicationsData(
            request.PacienteId,
            compraPacienteId,
            request.AplicadorId,
            request.UnidadeId,
            request.DataAplicacao,
            request.Peso,
            string.IsNullOrWhiteSpace(request.Observacao) ? null : request.Observacao.Trim(),
            sintomaIds,
            procedimentosValidados));
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
            vinculo.CanApply()
            && (
                (vinculo.EmpresaId.HasValue && vinculo.EmpresaId.Value == empresaId)
                || (vinculo.UnidadeId.HasValue && vinculo.UnidadeId.Value == unidadeId)));
    }
}
