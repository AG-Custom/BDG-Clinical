using AG.CLINICAL.Application.Core.Dtos;
using AG.CLINICAL.Application.Identity;
using AG.CLINICAL.Domain.Entities;

namespace AG.CLINICAL.Application.Core.Companies;

internal static class UserCompaniesMapper
{
    public static UserCompanyDto Map(Usuario usuario, Guid? currentEmpresaId = null)
    {
        return new UserCompanyDto(
            usuario.EmpresaId,
            usuario.Id,
            usuario.Empresa.Nome,
            usuario.Empresa.Logo,
            usuario.Empresa.CorPrincipal,
            usuario.Ativo && usuario.Empresa.Ativo,
            currentEmpresaId.HasValue && usuario.EmpresaId == currentEmpresaId.Value);
    }

    public static IReadOnlyList<UserCompanyDto> MapMany(
        IEnumerable<Usuario> usuarios,
        Guid? currentEmpresaId = null)
    {
        return usuarios
            .Select(usuario => Map(usuario, currentEmpresaId))
            .OrderBy(company => company.Nome)
            .ToList();
    }

    public static bool IsEligibleForAccess(Usuario usuario)
    {
        return usuario.Ativo
            && usuario.Empresa.Ativo
            && usuario.AuthProvider == IdentityConstants.AuthProviderLocal
            && !usuario.PendentePrimeiroAcesso
            && !string.IsNullOrWhiteSpace(usuario.SenhaHash);
    }
}
