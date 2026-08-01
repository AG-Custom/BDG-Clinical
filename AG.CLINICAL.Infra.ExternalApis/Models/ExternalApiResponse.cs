namespace AG.CLINICAL.Infra.ExternalApis.Models;

public sealed record ExternalApiResponse<TData>(TData? Data, bool IsSuccess, string? Error);
