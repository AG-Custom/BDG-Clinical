namespace AG.CLINICAL.Application.Identity.Dtos;

public sealed record UserDisplayNameDto(
    Guid Id,
    string Nome);

public sealed record ResolveUserDisplayNamesRequest(
    IReadOnlyList<Guid> Ids);
