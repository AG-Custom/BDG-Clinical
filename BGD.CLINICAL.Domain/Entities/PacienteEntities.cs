using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class Paciente : AggregateRoot
{
    private Paciente()
    {
    }

    private Paciente(
        Guid empresaId,
        Guid unidadeId,
        string nome,
        string? cpf,
        string? telefone,
        string? email,
        DateOnly? dataNascimento,
        string? observacao)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        UnidadeId = unidadeId;
        Nome = nome;
        Cpf = cpf;
        Telefone = telefone;
        Email = email;
        DataNascimento = dataNascimento;
        Observacao = observacao;
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

    public static Paciente Create(
        Guid empresaId,
        Guid unidadeId,
        string nome,
        string? cpf,
        string? telefone,
        string? email,
        DateOnly? dataNascimento,
        string? observacao)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do paciente.");
        }

        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade do paciente.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do paciente.");
        }

        return new Paciente(
            empresaId,
            unidadeId,
            nome.Trim(),
            cpf,
            telefone,
            email,
            dataNascimento,
            observacao);
    }

    public void UpdateDetails(
        Guid unidadeId,
        string nome,
        string? cpf,
        string? telefone,
        string? email,
        DateOnly? dataNascimento,
        string? observacao)
    {
        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade do paciente.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do paciente.");
        }

        UnidadeId = unidadeId;
        Nome = nome.Trim();
        Cpf = cpf;
        Telefone = telefone;
        Email = email;
        DataNascimento = dataNascimento;
        Observacao = observacao;
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

public sealed class Sintoma : AggregateRoot
{
    private Sintoma()
    {
    }

    private Sintoma(Guid empresaId, string nome)
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

    public static Sintoma Create(Guid empresaId, string nome)
    {
        if (empresaId == Guid.Empty)
        {
            throw new DomainException("Informe a empresa do sintoma.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do sintoma.");
        }

        return new Sintoma(empresaId, nome.Trim());
    }

    public void UpdateDetails(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do sintoma.");
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
