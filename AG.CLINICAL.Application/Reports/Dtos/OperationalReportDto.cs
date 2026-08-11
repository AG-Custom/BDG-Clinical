namespace AG.CLINICAL.Application.Reports.Dtos;

public sealed record MedicationUsageReportItemDto(Guid ProdutoId, string ProdutoNome, string UnidadeMedida, decimal Quantidade);
public sealed record AppointmentCountsReportDto(int Consulta, int Aplicacao, int Retorno, int Total);
public sealed record StockFinancialFlowReportDto(decimal Entradas, decimal Saidas, decimal Saldo);
public sealed record OperationalReportDto(DateOnly DataInicio, DateOnly DataFim, IReadOnlyList<MedicationUsageReportItemDto> MedicamentosUsados, AppointmentCountsReportDto Agendamentos, StockFinancialFlowReportDto FluxoEstoque);
