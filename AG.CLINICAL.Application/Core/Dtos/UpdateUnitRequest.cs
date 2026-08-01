namespace AG.CLINICAL.Application.Core.Dtos;

public sealed record UpdateUnitRequest(
    string Nome,
    string? Endereco);
