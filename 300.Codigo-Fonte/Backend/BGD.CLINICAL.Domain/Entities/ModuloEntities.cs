using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class ModuloSistema : AggregateRoot
{
    private ModuloSistema()
    {
    }

    public ModuloSistema(string nome, string codigo, string? descricao)
        : base(Guid.NewGuid())
    {
        Nome = nome;
        Codigo = codigo;
        Descricao = descricao;
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;
    public string Codigo { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; }

    public ICollection<LicencaModulo> Licencas { get; private set; } = [];
    public ICollection<PermissaoUsuario> PermissoesUsuario { get; private set; } = [];
}

public sealed class LicencaModulo : AggregateRoot
{
    private LicencaModulo()
    {
    }

    public LicencaModulo(Guid empresaId, Guid moduloId, StatusLicencaModulo status, DateTime dataInicio, DateTime? dataFim, decimal valor)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        ModuloId = moduloId;
        Status = status;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Valor = valor;
    }

    public Guid EmpresaId { get; private set; }
    public Guid ModuloId { get; private set; }
    public StatusLicencaModulo Status { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }
    public decimal Valor { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public ModuloSistema Modulo { get; private set; } = null!;
}

public sealed class PermissaoUsuario : AggregateRoot
{
    private PermissaoUsuario()
    {
    }

    public PermissaoUsuario(Guid usuarioId, Guid moduloId, bool podeVisualizar, bool podeCriar, bool podeEditar, bool podeExcluir)
        : base(Guid.NewGuid())
    {
        UsuarioId = usuarioId;
        ModuloId = moduloId;
        PodeVisualizar = podeVisualizar;
        PodeCriar = podeCriar;
        PodeEditar = podeEditar;
        PodeExcluir = podeExcluir;
    }

    public Guid UsuarioId { get; private set; }
    public Guid ModuloId { get; private set; }
    public bool PodeVisualizar { get; private set; }
    public bool PodeCriar { get; private set; }
    public bool PodeEditar { get; private set; }
    public bool PodeExcluir { get; private set; }

    public Usuario Usuario { get; private set; } = null!;
    public ModuloSistema Modulo { get; private set; } = null!;
}
