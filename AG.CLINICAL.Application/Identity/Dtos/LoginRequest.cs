namespace AG.CLINICAL.Application.Identity.Dtos;

public sealed record LoginRequest(string Email, string Senha, Guid? EmpresaId = null);
