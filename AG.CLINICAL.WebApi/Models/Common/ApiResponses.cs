namespace AG.CLINICAL.WebApi.Models.Common;

public sealed record ApiResponse<T>(T Data, bool Success, string? Message = null);
