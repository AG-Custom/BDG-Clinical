using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class Empresa : AggregateRoot
{
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
    public ICollection<Funcionario> Funcionarios { get; private set; } = [];
    public ICollection<Cargo> Cargos { get; private set; } = [];
    public ICollection<LicencaModulo> LicencasModulo { get; private set; } = [];
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
    }

    public Guid EmpresaId { get; private set; }
    public Guid? FuncionarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string EmailLogin { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public string AuthProvider { get; private set; } = string.Empty;
    public string? GoogleId { get; private set; }
    public TipoUsuario TipoUsuario { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Funcionario? Funcionario { get; private set; }
    public ICollection<PermissaoUsuario> Permissoes { get; private set; } = [];
}

public sealed class Funcionario : AggregateRoot
{
    private Funcionario()
    {
    }

    public Funcionario(Guid empresaId, Guid unidadeId, string nome, string? telefone, string? email, Guid? cargoId, bool flagAplicador)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        UnidadeId = unidadeId;
        Nome = nome;
        Telefone = telefone;
        Email = email;
        CargoId = cargoId;
        FlagAplicador = flagAplicador;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public Guid? CargoId { get; private set; }
    public bool FlagAplicador { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public Cargo? Cargo { get; private set; }
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
    public ICollection<Funcionario> Funcionarios { get; private set; } = [];
}
