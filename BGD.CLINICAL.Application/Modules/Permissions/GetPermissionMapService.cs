using BGD.CLINICAL.Application.Common;
using BGD.CLINICAL.Application.Modules.Abstractions;
using BGD.CLINICAL.Application.Modules.Dtos;
using BGD.CLINICAL.Application.Modules.Permissions;

namespace BGD.CLINICAL.Application.Modules.Permissions;

public interface IGetPermissionMapService
{
    Task<Result<IReadOnlyList<PermissionMapNodeDto>>> ExecuteAsync(CancellationToken cancellationToken = default);
}

public sealed class GetPermissionMapService : IGetPermissionMapService
{
    private readonly IPermissionCatalogRepository _catalogRepository;

    public GetPermissionMapService(IPermissionCatalogRepository catalogRepository)
    {
        _catalogRepository = catalogRepository;
    }

    public async Task<Result<IReadOnlyList<PermissionMapNodeDto>>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var catalog = await _catalogRepository.ListAllAsync(cancellationToken);
        var nodes = PermissionMapBuilder.BuildTree(catalog);
        return Result<IReadOnlyList<PermissionMapNodeDto>>.Success(nodes);
    }
}

internal static class PermissionMapBuilder
{
    public static IReadOnlyList<PermissionMapNodeDto> BuildTree(IReadOnlyList<Domain.Entities.PermissaoSistema> catalog)
    {
        var byKey = catalog.ToDictionary(permissao => permissao.Chave, StringComparer.OrdinalIgnoreCase);
        var childrenByParent = catalog
            .Where(permissao => !string.IsNullOrWhiteSpace(permissao.ChavePai))
            .GroupBy(permissao => permissao.ChavePai!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var roots = catalog
            .Where(permissao => string.IsNullOrWhiteSpace(permissao.ChavePai) || !byKey.ContainsKey(permissao.ChavePai!))
            .OrderBy(permissao => permissao.Ordem)
            .ThenBy(permissao => permissao.Chave)
            .Select(permissao => MapNode(permissao, childrenByParent))
            .ToList();

        return roots;
    }

    private static PermissionMapNodeDto MapNode(
        Domain.Entities.PermissaoSistema permissao,
        IReadOnlyDictionary<string, List<Domain.Entities.PermissaoSistema>> childrenByParent)
    {
        var children = childrenByParent.TryGetValue(permissao.Chave, out var childItems)
            ? childItems
                .OrderBy(item => item.Ordem)
                .ThenBy(item => item.Chave)
                .Select(item => MapNode(item, childrenByParent))
                .ToList()
            : [];

        return new PermissionMapNodeDto(
            permissao.Chave,
            permissao.Descricao,
            permissao.Categoria,
            permissao.ModuloCodigo,
            permissao.Ordem,
            permissao.ChavePai,
            children);
    }
}
