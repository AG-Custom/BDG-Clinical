using System.Text.RegularExpressions;
using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class Empresa : AggregateRoot
{
    private static readonly Regex HexColorPattern = new(
        "^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$",
        RegexOptions.Compiled);

    private Empresa()
    {
    }

    public Empresa(string nome, string? cnpj, string? telefone, string? email)
        : base(Guid.NewGuid())
    {
        Nome = nome;
        Cnpj = cnpj;
        Telefone = telefone;
        Email = email;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;
    public string? Cnpj { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Logo { get; private set; }
    public string? CorPrincipal { get; private set; }
    public bool Ativo { get; private set; }

    public ICollection<Unidade> Unidades { get; private set; } = [];
    public ICollection<Usuario> Usuarios { get; private set; } = [];
    public ICollection<FuncionarioVinculo> FuncionarioVinculos { get; private set; } = [];
    public ICollection<Cargo> Cargos { get; private set; } = [];
    public ICollection<LicencaModulo> LicencasModulo { get; private set; } = [];

    public void UpdateDetails(
        string nome,
        string? cnpj,
        string? telefone,
        string? email,
        string? corPrincipal,
        string? logo)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome da empresa.");
        }

        if (!string.IsNullOrWhiteSpace(corPrincipal) && !IsValidHexColor(corPrincipal))
        {
            throw new DomainException("Informe uma cor principal válida no formato hexadecimal (#RGB ou #RRGGBB).");
        }

        Nome = nome.Trim();
        Cnpj = string.IsNullOrWhiteSpace(cnpj) ? null : cnpj.Trim();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        CorPrincipal = string.IsNullOrWhiteSpace(corPrincipal) ? null : corPrincipal.Trim();
        Logo = string.IsNullOrWhiteSpace(logo) ? null : logo.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        Ativo = true;
        AtualizadoEm = DateTime.UtcNow;
    }

    private static bool IsValidHexColor(string corPrincipal)
    {
        return HexColorPattern.IsMatch(corPrincipal.Trim());
    }
}

public sealed class Unidade : AggregateRoot
{
    private Unidade()
    {
    }

    public Unidade(Guid empresaId, string nome, string? endereco)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        Nome = nome;
        Endereco = endereco;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Endereco { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;

    public void UpdateDetails(string nome, string? endereco)
    {
        Nome = nome;
        Endereco = endereco;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        Ativo = true;
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class Usuario : AggregateRoot
{
    private Usuario()
    {
    }

    public Usuario(Guid empresaId, Guid? funcionarioId, string nome, string emailLogin, string senhaHash, TipoUsuario tipoUsuario)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        FuncionarioId = funcionarioId;
        Nome = nome;
        EmailLogin = emailLogin;
        SenhaHash = senhaHash;
        TipoUsuario = tipoUsuario;
        AuthProvider = "LOCAL";
        Ativo = true;
        PendentePrimeiroAcesso = false;
    }

    public static Usuario CreatePendingFirstAccess(
        Guid empresaId,
        Guid funcionarioId,
        string nome,
        string emailLogin,
        TipoUsuario tipoUsuario = TipoUsuario.Funcionario)
    {
        return new Usuario
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            FuncionarioId = funcionarioId,
            Nome = nome,
            EmailLogin = emailLogin,
            SenhaHash = null,
            TipoUsuario = tipoUsuario,
            AuthProvider = "LOCAL",
            Ativo = true,
            PendentePrimeiroAcesso = true,
            CriadoEm = DateTime.UtcNow
        };
    }

    public Guid EmpresaId { get; private set; }
    public Guid? FuncionarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string EmailLogin { get; private set; } = string.Empty;
    public string? SenhaHash { get; private set; }
    public string AuthProvider { get; private set; } = string.Empty;
    public string? GoogleId { get; private set; }
    public TipoUsuario TipoUsuario { get; private set; }
    public bool Ativo { get; private set; }
    public bool PendentePrimeiroAcesso { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Funcionario? Funcionario { get; private set; }
    public ICollection<PermissaoUsuario> Permissoes { get; private set; } = [];

    public void UpdateProfile(string nome)
    {
        Nome = nome;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void SetTipoUsuario(TipoUsuario tipoUsuario)
    {
        TipoUsuario = tipoUsuario;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        Ativo = true;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void CompleteFirstAccess(string senhaHash)
    {
        if (!PendentePrimeiroAcesso)
        {
            throw new DomainException("Este usuário já concluiu o primeiro acesso.");
        }

        if (string.IsNullOrWhiteSpace(senhaHash))
        {
            throw new DomainException("Informe uma senha válida.");
        }

        SenhaHash = senhaHash;
        PendentePrimeiroAcesso = false;
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class Funcionario : AggregateRoot
{
    private Funcionario()
    {
    }

    private Funcionario(string nome, string? telefone, string? email)
        : base(Guid.NewGuid())
    {
        Nome = nome;
        Telefone = telefone;
        Email = email;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public bool Ativo { get; private set; }

    public ICollection<FuncionarioVinculo> Vinculos { get; private set; } = [];

    public static Funcionario Create(string nome, string? telefone, string? email)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do funcionário.");
        }

        return new Funcionario(nome.Trim(), telefone, email);
    }

    public void UpdateDetails(string nome, string? telefone, string? email)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do funcionário.");
        }

        Nome = nome.Trim();
        Telefone = telefone;
        Email = email;
        AtualizadoEm = DateTime.UtcNow;
    }

    public FuncionarioVinculo AddEmpresaVinculo(Guid empresaId, Guid? cargoId, bool flagAplicador)
    {
        var vinculo = FuncionarioVinculo.CreateForEmpresa(Id, empresaId, cargoId, flagAplicador);
        Vinculos.Add(vinculo);
        AtualizadoEm = DateTime.UtcNow;
        return vinculo;
    }

    public FuncionarioVinculo AddUnidadeVinculo(Guid unidadeId, Guid? cargoId, bool flagAplicador)
    {
        var vinculo = FuncionarioVinculo.CreateForUnidade(Id, unidadeId, cargoId, flagAplicador);
        Vinculos.Add(vinculo);
        AtualizadoEm = DateTime.UtcNow;
        return vinculo;
    }

    public void DeactivateVinculosInEmpresa(Guid empresaId)
    {
        foreach (var vinculo in Vinculos.Where(v => v.BelongsToEmpresa(empresaId) && v.Ativo))
        {
            vinculo.Deactivate();
        }

        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        foreach (var vinculo in Vinculos.Where(v => v.Ativo))
        {
            vinculo.Deactivate();
        }

        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        Ativo = true;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void ReactivateVinculosInEmpresa(Guid empresaId)
    {
        var vinculosInativos = Vinculos
            .Where(vinculo => vinculo.BelongsToEmpresa(empresaId) && !vinculo.Ativo)
            .ToList();

        if (vinculosInativos.Count == 0)
        {
            throw new DomainException("Não há vínculos inativos para reativar nesta empresa.");
        }

        foreach (var vinculo in vinculosInativos)
        {
            vinculo.Reactivate();
        }

        AtualizadoEm = DateTime.UtcNow;
    }

    public bool HasActiveVinculoInEmpresa(Guid empresaId)
    {
        return Vinculos.Any(v => v.Ativo && v.BelongsToEmpresa(empresaId));
    }
}

public sealed class FuncionarioVinculo : Entity
{
    private FuncionarioVinculo()
    {
    }

    private FuncionarioVinculo(
        Guid funcionarioId,
        Guid? empresaId,
        Guid? unidadeId,
        Guid? cargoId,
        bool flagAplicador)
        : base(Guid.NewGuid())
    {
        ValidateExclusiveLink(empresaId, unidadeId);

        FuncionarioId = funcionarioId;
        EmpresaId = empresaId;
        UnidadeId = unidadeId;
        CargoId = cargoId;
        FlagAplicador = flagAplicador;
        Ativo = true;
    }

    public Guid FuncionarioId { get; private set; }
    public Guid? EmpresaId { get; private set; }
    public Guid? UnidadeId { get; private set; }
    public Guid? CargoId { get; private set; }
    public bool FlagAplicador { get; private set; }
    public bool Ativo { get; private set; }

    public Funcionario Funcionario { get; private set; } = null!;
    public Empresa? Empresa { get; private set; }
    public Unidade? Unidade { get; private set; }
    public Cargo? Cargo { get; private set; }

    public static FuncionarioVinculo CreateForEmpresa(
        Guid funcionarioId,
        Guid empresaId,
        Guid? cargoId,
        bool flagAplicador)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do vínculo.");
        }

        return new FuncionarioVinculo(funcionarioId, empresaId, null, cargoId, flagAplicador);
    }

    public static FuncionarioVinculo CreateForUnidade(
        Guid funcionarioId,
        Guid unidadeId,
        Guid? cargoId,
        bool flagAplicador)
    {
        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade do vínculo.");
        }

        return new FuncionarioVinculo(funcionarioId, null, unidadeId, cargoId, flagAplicador);
    }

    public bool BelongsToEmpresa(Guid empresaId)
    {
        if (EmpresaId.HasValue)
        {
            return EmpresaId.Value == empresaId;
        }

        return Unidade?.EmpresaId == empresaId;
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        Ativo = true;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void UpdateAssignment(Guid? cargoId, bool flagAplicador)
    {
        CargoId = cargoId;
        FlagAplicador = flagAplicador;
        AtualizadoEm = DateTime.UtcNow;
    }

    private static void ValidateExclusiveLink(Guid? empresaId, Guid? unidadeId)
    {
        var hasEmpresa = empresaId.HasValue && empresaId.Value != Guid.Empty;
        var hasUnidade = unidadeId.HasValue && unidadeId.Value != Guid.Empty;

        if (hasEmpresa == hasUnidade)
        {
            throw new DomainException("O vínculo deve ser por empresa ou por unidade, não ambos.");
        }
    }
}

public sealed class Cargo : AggregateRoot
{
    private Cargo()
    {
    }

    public Cargo(Guid empresaId, string nome)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        Nome = nome;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public ICollection<FuncionarioVinculo> FuncionarioVinculos { get; private set; } = [];

    public void UpdateDetails(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do cargo.");
        }

        Nome = nome.Trim();
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Ativo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        Ativo = true;
        AtualizadoEm = DateTime.UtcNow;
    }
}
