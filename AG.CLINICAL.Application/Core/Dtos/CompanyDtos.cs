namespace AG.CLINICAL.Application.Core.Dtos;

public sealed record CompanyDto(
    Guid Id,
    string Nome,
    string? Cnpj,
    string? Telefone,
    string? Email,
    string? Logo,
    string? CorPrincipal,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record UpdateCompanyRequest(
    string Nome,
    string? Cnpj,
    string? Telefone,
    string? Email,
    string? CorPrincipal,
    string? Logo,
    bool Ativo);

public sealed record CreateCompanyRequest(
    string Nome,
    string? Cnpj,
    string? Telefone,
    string? Email,
    string? CorPrincipal);

public sealed record UserCompanyDto(
    Guid EmpresaId,
    Guid UsuarioId,
    string Nome,
    string? Logo,
    string? CorPrincipal,
    bool Ativo,
    bool IsCurrent);
