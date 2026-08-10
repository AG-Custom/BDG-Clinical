using AG.CLINICAL.Application.Abstractions.Identity;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Entities;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Identity.Users;

public sealed class ProvisionEmployeeUsersService : IProvisionEmployeeUsersService
{
    private readonly IUsersRepository _usersRepository;

    public ProvisionEmployeeUsersService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Usuario> ProvisionAsync(
        Guid empresaId,
        Guid funcionarioId,
        string nome,
        string emailLogin,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var tipoUsuario = isAdmin ? TipoUsuario.Admin : TipoUsuario.Funcionario;
        var usuario = Usuario.CreatePendingFirstAccess(
            empresaId,
            funcionarioId,
            nome,
            emailLogin,
            tipoUsuario);

        await _usersRepository.AddAsync(usuario, cancellationToken);
        return usuario;
    }

    public async Task DeactivateByFuncionarioAsync(
        Guid empresaId,
        Guid funcionarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usersRepository.GetByFuncionarioIdAndEmpresaIdAsync(
            funcionarioId,
            empresaId,
            cancellationToken);

        if (usuario is null || !usuario.Ativo)
        {
            return;
        }

        usuario.Deactivate();
        _usersRepository.Update(usuario);
    }

    public async Task ReactivateByFuncionarioAsync(
        Guid empresaId,
        Guid funcionarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usersRepository.GetByFuncionarioIdAndEmpresaIdAsync(
            funcionarioId,
            empresaId,
            cancellationToken);

        if (usuario is null)
        {
            throw new DomainException("Usuário de acesso do funcionário não encontrado.");
        }

        if (usuario.Ativo)
        {
            return;
        }

        usuario.Reactivate();
        _usersRepository.Update(usuario);
    }
}
