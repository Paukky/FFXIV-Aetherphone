using Aetherphone.Core.Aethernet.Contracts;

namespace Aetherphone.Core.Aethernet.Clients;

internal sealed class KupoClient
{
    private readonly AethernetTransport net;

    public KupoClient(AethernetTransport net)
    {
        this.net = net;
    }



    public Task<ConfessionDto?> CreateConfessionAsync(string text, int expiryDays, CancellationToken token,
        Action<int>? statusSink = null)
    {
        var now = DateTime.UtcNow;
        DateTime? expiresAt;
        switch (expiryDays)
        {
            case 1:
                expiresAt = now.AddDays(1);
                break;
            case 2:
                expiresAt = now.AddDays(3);
                break;
            case 3:
                expiresAt = now.AddDays(7);
                break;
            default:
                expiresAt = null;
                break;
        }
        return net.PostAsync("/kupo/confessions", new CreateConfessionRequest(text, expiresAt), AethernetJsonContext.Default.CreateConfessionRequest,
            AethernetJsonContext.Default.ConfessionDto, token, statusSink);
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

    public Task<ConfessionPage?> MyConfessionsAsync(string userId, string? cursor, CancellationToken token)
    {
        var path = $"/kupo/{userId}/confessions";

        if (cursor is not null)
        {
            path += $"?cursor={Uri.EscapeDataString(cursor)}";
        }

        return net.GetAsync(path, AethernetJsonContext.Default.ConfessionPage, token);
    }

    public Task<ResponseDto?> RespondAsync(string confessionId, CreateResponseRequest request,
        CancellationToken token, Action<int>? statusSink = null)
    {
        return net.PostAsync($"/kupo/confessions/{Uri.EscapeDataString(confessionId)}/responses", request,
            AethernetJsonContext.Default.CreateResponseRequest,
            AethernetJsonContext.Default.ResponseDto, token, statusSink);
    }

    public Task<ResponseDto?> RespondAsync(string confessionId, string content, CancellationToken token,
        Action<int>? statusSink = null)
    {
        return RespondAsync(confessionId, new CreateResponseRequest(content), token, statusSink);
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



    public Task<KindKupoInboxDto?> InboxAsync(string userId, CancellationToken token)
    {
        var path = $"/kupo/users/{Uri.EscapeDataString(userId)}/confessions";

        return net.GetAsync(path, AethernetJsonContext.Default.KindKupoInboxDto, token);
    }

    public Task<KindKupoInboxDto?> UserInboxAsync(string userId, CancellationToken token)
    {
        return net.GetAsync($"/kupo/users/{Uri.EscapeDataString(userId)}/inbox",
            AethernetJsonContext.Default.KindKupoInboxDto, token);
    }



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

    // Stats

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

