namespace AG.CLINICAL.Application.Identity.Dtos;

public sealed record RegisterCompanyRequest(
    string NomeEmpresa,
    string Nome,
    string Email,
    string Senha,
    string? Cnpj,
    string? Telefone,
    string? CorPrincipal);
