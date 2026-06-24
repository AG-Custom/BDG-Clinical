namespace BGD.CLINICAL.Application.Core.Dtos;

public sealed record CreateUnitRequest(
    string Nome,
    string? Endereco);
