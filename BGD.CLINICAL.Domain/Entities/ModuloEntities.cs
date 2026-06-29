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

    public bool IsActiveAt(DateTime referenceUtc)
    {
        if (Status is not (StatusLicencaModulo.Ativo or StatusLicencaModulo.Teste))
        {
            return false;
        }

        if (DataInicio > referenceUtc)
        {
            return false;
        }

        return DataFim is null || DataFim >= referenceUtc;
    }
}
