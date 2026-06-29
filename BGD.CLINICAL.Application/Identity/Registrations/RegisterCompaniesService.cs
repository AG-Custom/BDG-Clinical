using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Companies;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Identity.Dtos;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Identity.Registrations;

public sealed class RegisterCompaniesService : IRegisterCompaniesService
{
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly ICompaniesRepository _companiesRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashGenerator _passwordHashGenerator;
    private readonly ITokenService _tokenService;
    private readonly ICompanyModuleLicensesProvisioner _moduleLicensesProvisioner;
    private readonly ICompanyDefaultMeasurementUnitsProvisioner _defaultMeasurementUnitsProvisioner;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCompaniesService(
        IRepository<Empresa> empresaRepository,
        ICompaniesRepository companiesRepository,
        IUsersRepository usersRepository,
        IPasswordHashGenerator passwordHashGenerator,
        ITokenService tokenService,
        ICompanyModuleLicensesProvisioner moduleLicensesProvisioner,
        ICompanyDefaultMeasurementUnitsProvisioner defaultMeasurementUnitsProvisioner,
        IUnitOfWork unitOfWork)
    {
        _empresaRepository = empresaRepository;
        _companiesRepository = companiesRepository;
        _usersRepository = usersRepository;
        _passwordHashGenerator = passwordHashGenerator;
        _tokenService = tokenService;
        _moduleLicensesProvisioner = moduleLicensesProvisioner;
        _defaultMeasurementUnitsProvisioner = defaultMeasurementUnitsProvisioner;
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
        var cnpj = CompanyValidation.NormalizeCnpj(request.Cnpj);
        var telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim();
        var corPrincipal = string.IsNullOrWhiteSpace(request.CorPrincipal)
            ? null
            : request.CorPrincipal.Trim();

        if (await _usersRepository.ExistsActiveByEmailAsync(email, cancellationToken))
        {
            return Result<AuthResponse>.Failure("Já existe uma conta com este e-mail.");
        }

        if (cnpj is not null
            && await _companiesRepository.ExistsByCnpjAsync(cnpj, null, cancellationToken))
        {
            return Result<AuthResponse>.Failure("Já existe uma empresa cadastrada com este CNPJ.");
        }

        var empresa = new Empresa(
            request.NomeEmpresa.Trim(),
            cnpj,
            telefone,
            email);

        if (corPrincipal is not null)
        {
            try
            {
                empresa.UpdateDetails(
                    empresa.Nome,
                    empresa.Cnpj,
                    empresa.Telefone,
                    empresa.Email,
                    corPrincipal,
                    logo: null);
            }
            catch (DomainException ex)
            {
                return Result<AuthResponse>.Failure(ex.Message);
            }
        }

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

        await _moduleLicensesProvisioner.ProvisionAllModulesAsync(empresa.Id, cancellationToken);
        await _defaultMeasurementUnitsProvisioner.ProvisionDefaultMeasurementUnitsAsync(empresa.Id, cancellationToken);

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

        return CompanyValidation.ValidateCorPrincipal(request.CorPrincipal);
    }
}
