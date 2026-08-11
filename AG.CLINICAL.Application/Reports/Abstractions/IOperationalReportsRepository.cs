using AG.CLINICAL.Application.Reports.Dtos;

namespace AG.CLINICAL.Application.Reports.Abstractions;

public interface IOperationalReportsRepository
{
    Task<OperationalReportDto> GetAsync(Guid empresaId, DateOnly dataInicio, DateOnly dataFim, CancellationToken cancellationToken = default);
}
