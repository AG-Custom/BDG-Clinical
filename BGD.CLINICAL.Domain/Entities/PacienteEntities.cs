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
    public ICollection<AplicacaoPaciente> Aplicacoes { get; private set; } = [];
    public ICollection<PacienteUnidade> UnidadesVinculadas { get; private set; } = [];

    public static Paciente Create(
        Guid empresaId,
        IReadOnlyList<Guid> unidadeIds,
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

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do paciente.");
        }

        ValidateUnidadeIds(unidadeIds);

        var paciente = new Paciente(
            empresaId,
            unidadeIds[0],
            nome.Trim(),
            cpf,
            telefone,
            email,
            dataNascimento,
            observacao);

        paciente.SetUnidades(unidadeIds);
        return paciente;
    }

    public void UpdateDetails(
        IReadOnlyList<Guid> unidadeIds,
        string nome,
        string? cpf,
        string? telefone,
        string? email,
        DateOnly? dataNascimento,
        string? observacao)
    {
        ValidateUnidadeIds(unidadeIds);

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Informe o nome do paciente.");
        }

        UnidadeId = unidadeIds[0];
        Nome = nome.Trim();
        Cpf = cpf;
        Telefone = telefone;
        Email = email;
        DataNascimento = dataNascimento;
        Observacao = observacao;
        SetUnidades(unidadeIds);
        AtualizadoEm = DateTime.UtcNow;
    }

    public IReadOnlyList<Guid> GetUnidadeIds()
    {
        if (UnidadesVinculadas.Count > 0)
        {
            return UnidadesVinculadas
                .OrderBy(item => item.CriadoEm)
                .Select(item => item.UnidadeId)
                .ToList();
        }

        return UnidadeId != Guid.Empty
            ? [UnidadeId]
            : [];
    }

    public bool IsLinkedToUnidade(Guid unidadeId)
    {
        if (unidadeId == Guid.Empty)
        {
            return false;
        }

        return GetUnidadeIds().Contains(unidadeId);
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

    private void SetUnidades(IReadOnlyList<Guid> unidadeIds)
    {
        UnidadesVinculadas.Clear();

        foreach (var unidadeId in unidadeIds)
        {
            UnidadesVinculadas.Add(new PacienteUnidade(Id, unidadeId));
        }
    }

    private static void ValidateUnidadeIds(IReadOnlyList<Guid> unidadeIds)
    {
        var normalizedUnidadeIds = unidadeIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalizedUnidadeIds.Count == 0)
        {
            throw new DomainException("Informe ao menos uma unidade do paciente.");
        }

        if (normalizedUnidadeIds.Count != unidadeIds.Count(id => id != Guid.Empty))
        {
            throw new DomainException("Não é permitido repetir a mesma unidade no paciente.");
        }
    }
}

public sealed class PacienteUnidade : AggregateRoot
{
    private PacienteUnidade()
    {
    }

    public PacienteUnidade(Guid pacienteId, Guid unidadeId)
        : base(Guid.NewGuid())
    {
        if (pacienteId == Guid.Empty)
        {
            throw new DomainException("Informe o paciente da unidade.");
        }

        if (unidadeId == Guid.Empty)
        {
            throw new DomainException("Informe a unidade do paciente.");
        }

        PacienteId = pacienteId;
        UnidadeId = unidadeId;
    }

    public Guid PacienteId { get; private set; }
    public Guid UnidadeId { get; private set; }

    public Paciente Paciente { get; private set; } = null!;
    public Unidade Unidade { get; private set; } = null!;
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
