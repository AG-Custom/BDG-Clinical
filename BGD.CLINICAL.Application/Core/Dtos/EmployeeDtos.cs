namespace BGD.CLINICAL.Application.Core.Dtos;

public sealed record EmployeeLinkDto(
    Guid Id,
    Guid? EmpresaId,
    Guid? UnidadeId,
    Guid? CargoId,
    bool Ativo);

public sealed record EmployeeDto(
    Guid Id,
    string Nome,
    string? Telefone,
    string? Email,
    string EmailLogin,
    bool PendentePrimeiroAcesso,
    bool IsAdmin,
    bool Ativo,
    IReadOnlyList<EmployeeLinkDto> Links,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);

public sealed record CreateEmployeeRequest(
    string Nome,
    string? Telefone,
    string? Email,
    string EmailLogin,
    bool LinkToEmpresa,
    IReadOnlyList<Guid>? UnidadeIds,
    Guid? CargoId,
    bool IsAdmin = false);

public sealed record UpdateEmployeeRequest(
    string Nome,
    string? Telefone,
    string? Email,
    bool LinkToEmpresa,
    IReadOnlyList<Guid>? UnidadeIds,
    Guid? CargoId,
    bool IsAdmin = false);
