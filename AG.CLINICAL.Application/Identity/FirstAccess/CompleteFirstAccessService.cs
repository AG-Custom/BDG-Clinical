using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Identity.Dtos;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Identity.FirstAccess;

public interface ICompleteFirstAccessService
{
    Task<Result<AuthResponse>> ExecuteAsync(
        CompleteFirstAccessRequest request,
        string? ip,
        CancellationToken cancellationToken = default);
}

public sealed class CompleteFirstAccessService : ICompleteFirstAccessService
{
    private readonly IFirstAccessInvitationsRepository _invitationsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashGenerator _passwordHashGenerator;
    private readonly ITokenService _tokenService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteFirstAccessService(
        IFirstAccessInvitationsRepository invitationsRepository,
        IUsersRepository usersRepository,
        IPasswordHashGenerator passwordHashGenerator,
        ITokenService tokenService,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _invitationsRepository = invitationsRepository;
        _usersRepository = usersRepository;
        _passwordHashGenerator = passwordHashGenerator;
        _tokenService = tokenService;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(
        CompleteFirstAccessRequest request,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < IdentityConstants.SenhaMinimaCaracteres)
        {
            return Result<AuthResponse>.Failure(
                $"A senha deve ter no mínimo {IdentityConstants.SenhaMinimaCaracteres} caracteres.");
        }

        var resolved = await FirstAccessInvitationResolver.ResolveAsync(
            request.Token,
            request.Email,
            _invitationsRepository,
            cancellationToken);

        if (resolved.IsFailure)
        {
            return Result<AuthResponse>.Failure(resolved.Error!);
        }

        var convite = resolved.Value!;
        var usuario = convite.Usuario;

        try
        {
            var senhaHash = _passwordHashGenerator.GenerateHash(request.Senha);
            usuario.CompleteFirstAccess(senhaHash);
            convite.MarkAsUsed();

            _usersRepository.Update(usuario);
            _invitationsRepository.Update(convite);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var token = _tokenService.GenerateToken(usuario);
            await _auditLogsService.RegisterLoginAsync(usuario.Id, usuario.EmpresaId, ip, cancellationToken);

            return Result<AuthResponse>.Success(new AuthResponse(
                token,
                AuthenticatedUsersMapper.Map(usuario)));
        }
        catch (DomainException exception)
        {
            return Result<AuthResponse>.Failure(exception.Message);
        }
    }
}
