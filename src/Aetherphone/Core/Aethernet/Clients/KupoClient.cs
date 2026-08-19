using Aetherphone.Core.Aethernet.Contracts;

namespace Aetherphone.Core.Aethernet.Clients;

internal sealed class KupoClient
{
    private readonly AethernetTransport net;

    public KupoClient(AethernetTransport net)
    {
        this.net = net;
    }

    // --- Confessions ---

    public Task<ConfessionDto?> CreateConfessionAsync(CreateConfessionRequest request, CancellationToken token,
        Action<int>? statusSink = null)
    {
        return net.PostAsync("/kupo/confessions", request, AethernetJsonContext.Default.CreateConfessionRequest,
            AethernetJsonContext.Default.ConfessionDto, token, statusSink);
    }

    public Task<ConfessionDto?> CreateConfessionAsync(string content, int expiryDays, CancellationToken token,
        Action<int>? statusSink = null)
    {
        return CreateConfessionAsync(new CreateConfessionRequest(content, expiryDays), token, statusSink);
    }

    public Task<ConfessionDto?> GetConfessionAsync(string confessionId, CancellationToken token)
    {
        return net.GetAsync($"/kupo/confessions/{Uri.EscapeDataString(confessionId)}",
            AethernetJsonContext.Default.ConfessionDto, token);
    }

    public Task<bool> DeleteConfessionAsync(string confessionId, CancellationToken token)
    {
        return net.SendAsync(HttpMethod.Delete, $"/kupo/confessions/{Uri.EscapeDataString(confessionId)}", token);
    }

    public Task<ConfessionPage?> FeedAsync(string? cursor, CancellationToken token)
    {
        var path = "/kupo/feed";
        if (cursor is not null)
        {
            path += $"?cursor={Uri.EscapeDataString(cursor)}";
        }

        return net.GetAsync(path, AethernetJsonContext.Default.ConfessionPage, token);
    }

    public Task<ConfessionPage?> UserConfessionsAsync(string userId, string? cursor, CancellationToken token)
    {
        var path = $"/kupo/users/{Uri.EscapeDataString(userId)}/confessions";
        if (cursor is not null)
        {
            path += $"?cursor={Uri.EscapeDataString(cursor)}";
        }

        return net.GetAsync(path, AethernetJsonContext.Default.ConfessionPage, token);
    }

    public Task<ConfessionPage?> MyConfessionsAsync(string? cursor, CancellationToken token)
    {
        var path = "/kupo/me/confessions";
        if (cursor is not null)
        {
            path += $"?cursor={Uri.EscapeDataString(cursor)}";
        }

        return net.GetAsync(path, AethernetJsonContext.Default.ConfessionPage, token);
    }

    // --- Responses / Replies ---

    public Task<ConfessionResponseDto?> RespondAsync(string confessionId, CreateConfessionResponseRequest request,
        CancellationToken token, Action<int>? statusSink = null)
    {
        return net.PostAsync($"/kupo/confessions/{Uri.EscapeDataString(confessionId)}/responses", request,
            AethernetJsonContext.Default.CreateConfessionResponseRequest,
            AethernetJsonContext.Default.ConfessionResponseDto, token, statusSink);
    }

    public Task<ConfessionResponseDto?> RespondAsync(string confessionId, string content, CancellationToken token,
        Action<int>? statusSink = null)
    {
        return RespondAsync(confessionId, new CreateConfessionResponseRequest(content), token, statusSink);
    }

    public Task<ConfessionResponsePage?> ResponsesAsync(string confessionId, string? cursor, CancellationToken token)
    {
        var path = $"/kupo/confessions/{Uri.EscapeDataString(confessionId)}/responses";
        if (cursor is not null)
        {
            path += $"?cursor={Uri.EscapeDataString(cursor)}";
        }

        return net.GetAsync(path, AethernetJsonContext.Default.ConfessionResponsePage, token);
    }

    public Task<bool> DeleteResponseAsync(string confessionId, string responseId, CancellationToken token)
    {
        return net.SendAsync(HttpMethod.Delete,
            $"/kupo/confessions/{Uri.EscapeDataString(confessionId)}/responses/{Uri.EscapeDataString(responseId)}", token);
    }

    // --- Inbox ---

    public Task<KindKupoInboxDto?> InboxAsync(CancellationToken token)
    {
        return net.GetAsync("/kupo/inbox", AethernetJsonContext.Default.KindKupoInboxDto, token);
    }

    public Task<KindKupoInboxDto?> UserInboxAsync(string userId, CancellationToken token)
    {
        return net.GetAsync($"/kupo/users/{Uri.EscapeDataString(userId)}/inbox",
            AethernetJsonContext.Default.KindKupoInboxDto, token);
    }

    // --- Kudos & Reactions ---

    public Task<bool> SendKudosAsync(string confessionId, CancellationToken token)
    {
        return net.SendAsync(HttpMethod.Post, $"/kupo/confessions/{Uri.EscapeDataString(confessionId)}/kudos", token);
    }

    public Task<bool> LikeResponseAsync(string confessionId, string responseId, CancellationToken token)
    {
        return net.SendAsync(HttpMethod.Post,
            $"/kupo/confessions/{Uri.EscapeDataString(confessionId)}/responses/{Uri.EscapeDataString(responseId)}/like",
            token);
    }

    // --- Stats ---

    public Task<KindKupoStatsDto?> StatsAsync(CancellationToken token)
    {
        return net.GetAsync("/kupo/stats", AethernetJsonContext.Default.KindKupoStatsDto, token);
    }

    public Task<KindKupoStatsDto?> UserStatsAsync(string userId, CancellationToken token)
    {
        return net.GetAsync($"/kupo/users/{Uri.EscapeDataString(userId)}/stats",
            AethernetJsonContext.Default.KindKupoStatsDto, token);
    }
}

