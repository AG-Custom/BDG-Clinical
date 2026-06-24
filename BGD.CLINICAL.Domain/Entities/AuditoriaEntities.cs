using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class LogAuditoria : AggregateRoot
{
    private LogAuditoria()
    {
    }

    public LogAuditoria(
        Guid empresaId,
        Guid usuarioId,
        string entidade,
        Guid registroId,
        AcaoAuditoria acao,
        DateTime data,
        string? ip,
        string? dadosAnteriores = null,
        string? dadosNovos = null)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        UsuarioId = usuarioId;
        Entidade = entidade;
        RegistroId = registroId;
        Acao = acao;
        Data = data;
        Ip = ip;
        DadosAnteriores = dadosAnteriores;
        DadosNovos = dadosNovos;
    }

    public Guid EmpresaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Entidade { get; private set; } = string.Empty;
    public Guid RegistroId { get; private set; }
    public AcaoAuditoria Acao { get; private set; }
    public string? DadosAnteriores { get; private set; }
    public string? DadosNovos { get; private set; }
    public DateTime Data { get; private set; }
    public string? Ip { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;
}
