namespace Aetherphone.Core.Aethernet.Contracts;

internal sealed record ConfessionDto(
    string Id,
    string AuthorId,
    string Text,
    DateTime CreatedAt,
    int ResponseCount,
    List<ResponseDto> Responses
    );

internal sealed record ResponseDto(
    string Id,
    string ConfessionId,
    string ResponderId,
    string Text,
    DateTime CreatedAt
    );

internal sealed record ConfessionFeeds(ConfessionDto[] Confessions);

internal sealed record KindKupoInboxDto(string AccountId, List<ConfessionDto> KupoInboxes);

internal sealed record CreateConfessionRequest(string Text, DateTime? ExpiresAt);

internal sealed record CreateResponseRequest(string Text);

internal sealed record ConfessionPage(ConfessionDto[] Items, string? NextCursor);

internal sealed record ConfessionResponsePage(ResponseDto[] Items, string? NextCursor);

internal sealed record KindKupoStatsDto(int WrittenCount, int ResponseCount, int KudosCount);

