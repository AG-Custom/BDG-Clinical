using BGD.CLINICAL.Domain.Common;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class Paciente : AggregateRoot
{
    private Paciente()
    {
    }

    public Paciente(Guid empresaId, Guid unidadeId, string nome, string? cpf, string? telefone, string? email, DateOnly? dataNascimento)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        UnidadeId = unidadeId;
        Nome = nome;
        Cpf = cpf;
        Telefone = telefone;
        Email = email;
        DataNascimento = dataNascimento;
        Ativo = true;
    }

    public Guid EmpresaId { get; private set; }
    public Guid UnidadeId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string? Cpf { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public DateOnly? DataNascimento { get; private set; }
    public string? Observacao { get; private set; }
    public bool Ativo { get; private set; }

    public Empresa Empresa { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
    public ICollection<CompraPaciente> Compras { get; private set; } = [];
    public ICollection<AplicacaoPaciente> Aplicacoes { get; private set; } = [];
}

public sealed class Sintoma : AggregateRoot
{
    private Sintoma()
    {
    }

    public Sintoma(Guid empresaId, string nome)
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
    public ICollection<AplicacaoSintoma> Aplicacoes { get; private set; } = [];
}
