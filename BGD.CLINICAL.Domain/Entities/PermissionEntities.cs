using BGD.CLINICAL.Domain.Common;
using BGD.CLINICAL.Domain.Enums;
using BGD.CLINICAL.Domain.Exceptions;

namespace BGD.CLINICAL.Domain.Entities;

public sealed class PermissaoSistema : AggregateRoot
{
    private PermissaoSistema()
    {
    }

    public PermissaoSistema(
        string chave,
        string descricao,
        string categoria,
        string moduloCodigo,
        int ordem,
        string? chavePai)
        : base(Guid.NewGuid())
    {
        Chave = chave;
        Descricao = descricao;
        Categoria = categoria;
        ModuloCodigo = moduloCodigo;
        Ordem = ordem;
        ChavePai = chavePai;
    }

    public string Chave { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Categoria { get; private set; } = string.Empty;
    public string ModuloCodigo { get; private set; } = string.Empty;
    public int Ordem { get; private set; }
    public string? ChavePai { get; private set; }

    public void UpdateMetadata(string descricao, string categoria, string moduloCodigo, int ordem, string? chavePai)
    {
        Descricao = descricao;
        Categoria = categoria;
        ModuloCodigo = moduloCodigo;
        Ordem = ordem;
        ChavePai = chavePai;
        AtualizadoEm = DateTime.UtcNow;
    }
}

public sealed class CargoPermissaoItem : Entity
{
    private CargoPermissaoItem()
    {
    }

    public CargoPermissaoItem(Guid cargoId, string permissionKey)
        : base(Guid.NewGuid())
    {
        CargoId = cargoId;
        PermissionKey = permissionKey;
    }

    public Guid CargoId { get; private set; }
    public string PermissionKey { get; private set; } = string.Empty;

    public Cargo Cargo { get; private set; } = null!;
}

public sealed class UsuarioPermissaoOverride : Entity
{
    private UsuarioPermissaoOverride()
    {
    }

    public UsuarioPermissaoOverride(Guid usuarioId, string permissionKey, PermissionEffect effect)
        : base(Guid.NewGuid())
    {
        UsuarioId = usuarioId;
        PermissionKey = permissionKey;
        Effect = effect;
    }

    public Guid UsuarioId { get; private set; }
    public string PermissionKey { get; private set; } = string.Empty;
    public PermissionEffect Effect { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    public void UpdateEffect(PermissionEffect effect)
    {
        Effect = effect;
        AtualizadoEm = DateTime.UtcNow;
    }
}
