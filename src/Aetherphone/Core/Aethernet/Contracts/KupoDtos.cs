namespace Aetherphone.Core.Aethernet.Contracts;

internal sealed record ConfessionDto(
    string Id,
    string AuthorId,
    string Text,
    DateTime CreatedAt,
    int ResponseCount,
    List<ConfessionResponseDto> Responses
    );

internal sealed record ConfessionResponseDto(
    string Id,
    string ConfessionId,
    string ResponderId,
    string Text,
    DateTime CreatedAt
    );

internal sealed record ConfessionFeeds(ConfessionDto[] Confessions);

internal sealed record KindKupoInboxDto(string AccountId, List<ConfessionDto> KupoInboxes);

internal sealed record CreateConfessionRequest(string Text, int ExpiryDays = 0);

internal sealed record CreateConfessionResponseRequest(string Text);

internal sealed record ConfessionPage(ConfessionDto[] Items, string? NextCursor);

internal sealed record ConfessionResponsePage(ConfessionResponseDto[] Items, string? NextCursor);

internal sealed record KindKupoStatsDto(int WrittenCount, int ResponseCount, int KudosCount);

