using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Identity.Dtos;

namespace AG.CLINICAL.Application.Identity.FirstAccess;

public interface IValidateFirstAccessEmailsService
{
    Task<Result<ValidateFirstAccessEmailResponse>> ExecuteAsync(
        ValidateFirstAccessEmailRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class ValidateFirstAccessEmailsService : IValidateFirstAccessEmailsService
{
    private readonly IFirstAccessInvitationsRepository _invitationsRepository;

    public ValidateFirstAccessEmailsService(IFirstAccessInvitationsRepository invitationsRepository)
    {
        _invitationsRepository = invitationsRepository;
    }

    public async Task<Result<ValidateFirstAccessEmailResponse>> ExecuteAsync(
        ValidateFirstAccessEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var resolved = await FirstAccessInvitationResolver.ResolveAsync(
            request.Token,
            request.Email,
            _invitationsRepository,
            cancellationToken);

        if (resolved.IsFailure)
        {
            return Result<ValidateFirstAccessEmailResponse>.Failure(resolved.Error!);
        }

        var usuario = resolved.Value!.Usuario;

        return Result<ValidateFirstAccessEmailResponse>.Success(
            new ValidateFirstAccessEmailResponse(usuario.Nome, usuario.EmailLogin));
    }
}
