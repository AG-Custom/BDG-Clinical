using AG.CLINICAL.Application.Abstractions.Persistence;
using AG.CLINICAL.Application.Abstractions.Security;
using AG.CLINICAL.Application.Common;
using AG.CLINICAL.Application.Core.Abstractions;
using AG.CLINICAL.Application.Core.Dtos;
using AG.CLINICAL.Application.Identity;
using AG.CLINICAL.Application.Identity.Abstractions;
using AG.CLINICAL.Domain.Enums;
using AG.CLINICAL.Domain.Exceptions;

namespace AG.CLINICAL.Application.Core.Companies;

public interface IUpdateCurrentCompanyService
{
    Task<Result<CompanyDto>> ExecuteAsync(
        UpdateCompanyRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class UpdateCurrentCompanyService : IUpdateCurrentCompanyService
{
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ICompaniesRepository _companiesRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IAuditLogsService _auditLogsService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCurrentCompanyService(
        ICurrentTenantContext tenantContext,
        ICompaniesRepository companiesRepository,
        IUsersRepository usersRepository,
        IAuditLogsService auditLogsService,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _companiesRepository = companiesRepository;
        _usersRepository = usersRepository;
        _auditLogsService = auditLogsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CompanyDto>> ExecuteAsync(
        UpdateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return Result<CompanyDto>.Failure(validationError);
        }

        var empresaId = _tenantContext.EmpresaId;
        var empresa = await _companiesRepository.GetByIdAsync(empresaId, cancellationToken);

        if (empresa is null)
        {
            return Result<CompanyDto>.Failure("Empresa não encontrada.");
        }

        var usuarioAtual = await _usersRepository.GetByIdAsync(_tenantContext.UsuarioId, cancellationToken);
        var isAdmin = usuarioAtual is not null
            && usuarioAtual.Ativo
            && usuarioAtual.TipoUsuario == TipoUsuario.Admin;

        if (!empresa.Ativo)
        {
            if (!request.Ativo)
            {
                return Result<CompanyDto>.Failure("Esta clínica já está inativa.");
            }

            if (!isAdmin)
            {
                return Result<CompanyDto>.Failure("Somente administradores podem reativar a clínica.");
            }
        }

        var cnpj = CompanyValidation.NormalizeCnpj(request.Cnpj);

        if (cnpj is not null
            && await _companiesRepository.ExistsByCnpjAsync(cnpj, empresaId, cancellationToken))
        {
            return Result<CompanyDto>.Failure("Já existe uma empresa cadastrada com este CNPJ.");
        }

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        var telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim();
        var corPrincipal = string.IsNullOrWhiteSpace(request.CorPrincipal)
            ? null
            : request.CorPrincipal.Trim();
        var logo = request.Logo is null
            ? empresa.Logo
            : string.IsNullOrWhiteSpace(request.Logo) ? null : request.Logo.Trim();

        var dadosAnteriores = CompaniesAuditSerializer.Serialize(empresa);

        try
        {
            empresa.UpdateDetails(
                request.Nome.Trim(),
                cnpj,
                telefone,
                email,
                corPrincipal,
                logo);
        }
        catch (DomainException ex)
        {
            return Result<CompanyDto>.Failure(ex.Message);
        }

        if (!request.Ativo)
        {
            empresa.Deactivate();
        }
        else if (!empresa.Ativo)
        {
            empresa.Reactivate();
        }

        _companiesRepository.Update(empresa);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogsService.RegisterEntityChangeAsync(
            empresaId,
            _tenantContext.UsuarioId,
            nameof(Domain.Entities.Empresa),
            empresa.Id,
            AcaoAuditoria.Editar,
            dadosAnteriores: dadosAnteriores,
            dadosNovos: CompaniesAuditSerializer.Serialize(empresa),
            cancellationToken: cancellationToken);

        return Result<CompanyDto>.Success(CompaniesMapper.Map(empresa));
    }

    private static string? ValidateRequest(UpdateCompanyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return "Informe o nome da empresa.";
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && !IdentityValidation.IsValidEmail(request.Email))
        {
            return "Informe um e-mail válido.";
        }

        return CompanyValidation.ValidateCorPrincipal(request.CorPrincipal);
    }
}
