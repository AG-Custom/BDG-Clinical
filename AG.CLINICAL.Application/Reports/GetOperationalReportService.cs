using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Reports.Abstractions;
using AG.CLINICAL.Application.Reports.Dtos;

namespace AG.CLINICAL.Application.Reports;

public interface IGetOperationalReportService
{
    Task<Result<OperationalReportDto>> ExecuteAsync(DateOnly? dataInicio, DateOnly? dataFim, CancellationToken cancellationToken = default);
}

public sealed class GetOperationalReportService(ICurrentTenantContext tenantContext, IOperationalReportsRepository repository) : IGetOperationalReportService
{
    public async Task<Result<OperationalReportDto>> ExecuteAsync(DateOnly? dataInicio, DateOnly? dataFim, CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicioSemana = hoje.AddDays(-(((int)hoje.DayOfWeek + 6) % 7));
        var inicio = dataInicio ?? inicioSemana;
        var fim = dataFim ?? inicioSemana.AddDays(6);

        if (fim < inicio)
            return Result<OperationalReportDto>.Failure("A data final deve ser igual ou posterior à data inicial.");
        if (fim.DayNumber - inicio.DayNumber > 366)
            return Result<OperationalReportDto>.Failure("O período máximo permitido é de 367 dias.");

        return Result<OperationalReportDto>.Success(await repository.GetAsync(tenantContext.EmpresaId, inicio, fim, cancellationToken));
    }
}
