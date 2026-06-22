using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Identity.Dtos;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Application.Identity.Registrations;

public sealed class RegisterCompaniesService : IRegisterCompaniesService
{
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashGenerator _passwordHashGenerator;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCompaniesService(
        IRepository<Empresa> empresaRepository,
        IUsersRepository usersRepository,
        IPasswordHashGenerator passwordHashGenerator,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _empresaRepository = empresaRepository;
        _usersRepository = usersRepository;
        _passwordHashGenerator = passwordHashGenerator;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(
        RegisterCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var validacao = ValidateRequest(request);
        if (validacao is not null)
        {
            return Result<AuthResponse>.Failure(validacao);
        }

        var email = IdentityValidation.NormalizeEmail(request.Email);

        if (await _usersRepository.ExistsActiveByEmailAsync(email, cancellationToken))
        {
            return Result<AuthResponse>.Failure("Já existe uma conta com este e-mail.");
        }

        var empresa = new Empresa(
            request.NomeEmpresa.Trim(),
            string.IsNullOrWhiteSpace(request.Cnpj) ? null : request.Cnpj.Trim(),
            null,
            email);

        var senhaHash = _passwordHashGenerator.GenerateHash(request.Senha);
        var usuario = new Usuario(
            empresa.Id,
            null,
            request.Nome.Trim(),
            email,
            senhaHash,
            TipoUsuario.Admin);

        await _empresaRepository.AddAsync(empresa, cancellationToken);
        await _usersRepository.AddAsync(usuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _tokenService.GenerateToken(usuario);

        return Result<AuthResponse>.Success(new AuthResponse(
            token,
            AuthenticatedUsersMapper.Map(usuario)));
    }

    private static string? ValidateRequest(RegisterCompanyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NomeEmpresa))
        {
            return "Informe o nome da clínica.";
        }

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return "Informe o seu nome.";
        }

        if (!IdentityValidation.IsValidEmail(request.Email))
        {
            return "Informe um e-mail válido.";
        }

        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < IdentityConstants.SenhaMinimaCaracteres)
        {
            return $"A senha deve ter no mínimo {IdentityConstants.SenhaMinimaCaracteres} caracteres.";
        }

        return null;
    }
}
