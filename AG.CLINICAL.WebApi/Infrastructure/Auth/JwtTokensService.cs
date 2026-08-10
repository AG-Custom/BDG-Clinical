using AG.CLINICAL.Application.Identity;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AG.CLINICAL.WebApi.Infrastructure.Auth;

public sealed class JwtTokensService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokensService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.EmailLogin),
            new(JwtRegisteredClaimNames.Name, usuario.Nome),
            new(IdentityConstants.ClaimEmpresaId, usuario.EmpresaId.ToString()),
            new(IdentityConstants.ClaimTipoUsuario, usuario.TipoUsuario.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
