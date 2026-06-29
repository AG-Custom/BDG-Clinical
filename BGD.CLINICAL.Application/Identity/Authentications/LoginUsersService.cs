using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Companies;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Identity.Dtos;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Domain.Entities;

namespace BGD.CLINICAL.Application.Identity.Authentications;

public sealed class LoginUsersService : ILoginUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashGenerator _passwordHashGenerator;
    private readonly ITokenService _tokenService;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IPermissionChecker _permissionChecker;

    public LoginUsersService(
        IUsersRepository usersRepository,
        IPasswordHashGenerator passwordHashGenerator,
        ITokenService tokenService,
        IAuditLogsService auditLogsService,
        IPermissionChecker permissionChecker)
    {
        _usersRepository = usersRepository;
        _passwordHashGenerator = passwordHashGenerator;
        _tokenService = tokenService;
        _auditLogsService = auditLogsService;
        _permissionChecker = permissionChecker;
    }

    public async Task<Result<LoginResponse>> ExecuteAsync(
        LoginRequest request,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
        {
            return Result<LoginResponse>.Failure(IdentityValidation.CredenciaisInvalidas);
        }

        var email = IdentityValidation.NormalizeEmail(request.Email);
        var usuarios = await _usersRepository.ListByEmailLoginAsync(email, cancellationToken);

        var candidatos = usuarios
            .Where(UserCompaniesMapper.IsEligibleForAccess)
            .ToList();

        if (candidatos.Count == 0)
        {
            var pendentes = usuarios.Where(usuario => usuario.Ativo && usuario.PendentePrimeiroAcesso).ToList();
            if (pendentes.Count > 0)
            {
                return Result<LoginResponse>.Failure(IdentityValidation.PrimeiroAcessoPendente);
            }

            return Result<LoginResponse>.Failure(IdentityValidation.CredenciaisInvalidas);
        }

        var senhaValida = candidatos.Any(usuario =>
            !string.IsNullOrWhiteSpace(usuario.SenhaHash)
            && _passwordHashGenerator.Verify(request.Senha, usuario.SenhaHash));

        if (!senhaValida)
        {
            return Result<LoginResponse>.Failure(IdentityValidation.CredenciaisInvalidas);
        }

        if (request.EmpresaId is null && candidatos.Count > 1)
        {
            return Result<LoginResponse>.Success(new LoginResponse(
                RequiresCompanySelection: true,
                Token: null,
                Usuario: null,
                Companies: UserCompaniesMapper.MapMany(candidatos)));
        }

        var usuario = ResolveUsuario(candidatos, request.EmpresaId);

        if (usuario is null)
        {
            return Result<LoginResponse>.Failure(IdentityValidation.CredenciaisInvalidas);
        }

        var token = _tokenService.GenerateToken(usuario);
        await _auditLogsService.RegisterLoginAsync(usuario.Id, usuario.EmpresaId, ip, cancellationToken);

        var permissions = await _permissionChecker.GetEffectivePermissionsAsync(usuario.Id, cancellationToken);

        return Result<LoginResponse>.Success(new LoginResponse(
            RequiresCompanySelection: false,
            Token: token,
            Usuario: AuthenticatedUsersMapper.Map(usuario, permissions.OrderBy(key => key).ToList()),
            Companies: null));
    }

    private static Usuario? ResolveUsuario(IReadOnlyList<Usuario> candidatos, Guid? empresaId)
    {
        if (empresaId.HasValue)
        {
            return candidatos.FirstOrDefault(usuario => usuario.EmpresaId == empresaId.Value);
        }

        return candidatos.Count == 1 ? candidatos[0] : null;
    }
}
