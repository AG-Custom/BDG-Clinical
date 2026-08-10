namespace AG.CLINICAL.Application.Identity.Dtos;

public sealed record ValidateFirstAccessEmailRequest(string Token, string Email);

public sealed record ValidateFirstAccessEmailResponse(string Nome, string Email);

public sealed record CompleteFirstAccessRequest(string Token, string Email, string Senha);
