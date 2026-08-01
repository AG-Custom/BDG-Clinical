using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Companies;
using AG.CLINICAL.Application.Identity;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Application.Identity.Dtos;

using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Identity.Authentications;

public interface ISwitchCompanyService
{
    Task<Result<AuthResponse>> ExecuteAsync(
        SwitchCompanyRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class SwitchCompanyService : ISwitchCompanyService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IUsersRepository _usersRepository;
    private readonly ITokenService _tokenService;

    public SwitchCompanyService(
        ICurrentTenantContext tenantContext,
        IUsersRepository usersRepository,
        ITokenService tokenService)
    {
        _tenantContext = tenantContext;
        _usersRepository = usersRepository;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(
        SwitchCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuarioAtual = await _usersRepository.GetByIdAsync(_tenantContext.UsuarioId, cancellationToken);

        if (usuarioAtual is null || !usuarioAtual.Ativo)
        {
            return Result<AuthResponse>.Failure("Usuário não autenticado.");
        }

        if (request.EmpresaId == usuarioAtual.EmpresaId)
        {
            var currentToken = _tokenService.GenerateToken(usuarioAtual);
            return Result<AuthResponse>.Success(new AuthResponse(
                currentToken,
                AuthenticatedUsersMapper.Map(usuarioAtual)));
        }

        var usuarioDestino = await _usersRepository.GetByEmailLoginAndEmpresaIdAsync(
            usuarioAtual.EmailLogin,
            request.EmpresaId,
            cancellationToken);

        if (usuarioDestino is null || !CanSwitchToCompany(usuarioDestino))
        {
            return Result<AuthResponse>.Failure("Você não tem acesso a esta clínica.");
        }

        var token = _tokenService.GenerateToken(usuarioDestino);

        return Result<AuthResponse>.Success(new AuthResponse(
            token,
            AuthenticatedUsersMapper.Map(usuarioDestino)));
    }

    private static bool CanSwitchToCompany(Usuario usuario)
    {
        if (!usuario.Ativo
            || usuario.AuthProvider != IdentityConstants.AuthProviderLocal
            || usuario.PendentePrimeiroAcesso
            || string.IsNullOrWhiteSpace(usuario.SenhaHash))
        {
            return false;
        }

        if (usuario.Empresa.Ativo)
        {
            return true;
        }

        return usuario.TipoUsuario == TipoUsuario.Admin;
    }
}
