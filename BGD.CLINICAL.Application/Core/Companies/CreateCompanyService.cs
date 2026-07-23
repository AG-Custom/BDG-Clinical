using BGD.CLINICAL.Application.Abstractions.Persistence;
using BGD.CLINICAL.Application.Abstractions.Security;
using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Core.Abstractions;
using BGD.CLINICAL.Application.Core.Dtos;
using BGD.CLINICAL.Application.Identity;
using BGD.CLINICAL.Application.Identity.Abstractions;
using BGD.CLINICAL.Application.Identity.Dtos;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Application.Inventory.Abstractions;
using BGD.CLINICAL.Domain.Entities;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Application.Core.Companies;

public interface ICreateCompanyService
{
    Task<Result<AuthResponse>> ExecuteAsync(
        CreateCompanyRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class CreateCompanyService : ICreateCompanyService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly IRepository<Empresa> _empresaRepository;
    private readonly ICompaniesRepository _companiesRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly ITokenService _tokenService;
    private readonly ICompanyModuleLicensesProvisioner _moduleLicensesProvisioner;
    private readonly ICompanyDefaultMeasurementUnitsProvisioner _defaultMeasurementUnitsProvisioner;
    private readonly ICompanyDefaultProductTypesProvisioner _defaultProductTypesProvisioner;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCompanyService(
        ICurrentTenantContext tenantContext,
        IRepository<Empresa> empresaRepository,
        ICompaniesRepository companiesRepository,
        IUsersRepository usersRepository,
        ITokenService tokenService,
        ICompanyModuleLicensesProvisioner moduleLicensesProvisioner,
        ICompanyDefaultMeasurementUnitsProvisioner defaultMeasurementUnitsProvisioner,
        ICompanyDefaultProductTypesProvisioner defaultProductTypesProvisioner,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _empresaRepository = empresaRepository;
        _companiesRepository = companiesRepository;
        _usersRepository = usersRepository;
        _tokenService = tokenService;
        _moduleLicensesProvisioner = moduleLicensesProvisioner;
        _defaultMeasurementUnitsProvisioner = defaultMeasurementUnitsProvisioner;
        _defaultProductTypesProvisioner = defaultProductTypesProvisioner;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> ExecuteAsync(
        CreateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return Result<AuthResponse>.Failure(validationError);
        }

        var usuarioAtual = await _usersRepository.GetByIdAsync(_tenantContext.UsuarioId, cancellationToken);

        if (usuarioAtual is null || !usuarioAtual.Ativo)
        {
            return Result<AuthResponse>.Failure("Usuário não autenticado.");
        }

        if (usuarioAtual.PendentePrimeiroAcesso || string.IsNullOrWhiteSpace(usuarioAtual.SenhaHash))
        {
            return Result<AuthResponse>.Failure("Conclua o primeiro acesso antes de criar uma nova clínica.");
        }

        var cnpj = CompanyValidation.NormalizeCnpj(request.Cnpj);
        var telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim();
        var corPrincipal = string.IsNullOrWhiteSpace(request.CorPrincipal)
            ? null
            : request.CorPrincipal.Trim();
        var emailEmpresa = string.IsNullOrWhiteSpace(request.Email)
            ? usuarioAtual.EmailLogin
            : request.Email.Trim();

        if (cnpj is not null
            && await _companiesRepository.ExistsByCnpjAsync(cnpj, null, cancellationToken))
        {
            return Result<AuthResponse>.Failure("Já existe uma empresa cadastrada com este CNPJ.");
        }

        var empresa = new Empresa(
            request.Nome.Trim(),
            cnpj,
            telefone,
            emailEmpresa);

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

        var novoUsuario = new Usuario(
            empresa.Id,
            null,
            usuarioAtual.Nome,
            usuarioAtual.EmailLogin,
            usuarioAtual.SenhaHash,
            TipoUsuario.Admin);

        await _empresaRepository.AddAsync(empresa, cancellationToken);
        await _usersRepository.AddAsync(novoUsuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _moduleLicensesProvisioner.ProvisionAllModulesAsync(empresa.Id, cancellationToken);
        await _defaultMeasurementUnitsProvisioner.ProvisionDefaultMeasurementUnitsAsync(empresa.Id, cancellationToken);
        await _defaultProductTypesProvisioner.ProvisionDefaultProductTypesAsync(empresa.Id, cancellationToken);

        var token = _tokenService.GenerateToken(novoUsuario);

        return Result<AuthResponse>.Success(new AuthResponse(
            token,
            AuthenticatedUsersMapper.Map(novoUsuario)));
    }

    private static string? ValidateRequest(CreateCompanyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return "Informe o nome da clínica.";
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && !IdentityValidation.IsValidEmail(request.Email))
        {
            return "Informe um e-mail válido para a clínica.";
        }

        return CompanyValidation.ValidateCorPrincipal(request.CorPrincipal);
    }
}
