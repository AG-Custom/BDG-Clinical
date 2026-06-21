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

    public LicencaModulo(Guid empresaId, Guid moduloId, StatusLicencaModulo status, DateTime dataInicio, DateTime? dataFim)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        ModuloId = moduloId;
        Status = status;
        DataInicio = dataInicio;
        DataFim = dataFim;
    }

    public Guid EmpresaId { get; private set; }
    public Guid ModuloId { get; private set; }
    public StatusLicencaModulo Status { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public ModuloSistema Modulo { get; private set; } = null!;
}

public sealed class PermissaoUsuario : AggregateRoot
{
    private PermissaoUsuario()
    {
    }

    public PermissaoUsuario(Guid usuarioId, Guid moduloId, bool visualizar, bool criar, bool editar, bool excluir)
        : base(Guid.NewGuid())
    {
        UsuarioId = usuarioId;
        ModuloId = moduloId;
        Visualizar = visualizar;
        Criar = criar;
        Editar = editar;
        Excluir = excluir;
    }

    public Guid UsuarioId { get; private set; }
    public Guid ModuloId { get; private set; }
    public bool Visualizar { get; private set; }
    public bool Criar { get; private set; }
    public bool Editar { get; private set; }
    public bool Excluir { get; private set; }

    public Usuario Usuario { get; private set; } = null!;
    public ModuloSistema Modulo { get; private set; } = null!;
}
