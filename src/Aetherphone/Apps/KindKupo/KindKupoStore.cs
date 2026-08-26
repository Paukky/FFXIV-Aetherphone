using Aetherphone.Core;
using Aetherphone.Core.Aethernet;
using Aetherphone.Core.Aethernet.Clients;
using Aetherphone.Core.Aethernet.Contracts;
using Aetherphone.Core.Notifications;
using Aetherphone.Core.Social;

namespace Aetherphone.Apps.KindKupo;


// used for holding api call function and store is a container that persists within the app.
internal sealed class KindKupoStore : IDisposable
{
    private readonly AethernetSession session;
    private readonly KupoClient client;
    private readonly StoreWork work = new("KindKupo");
    private volatile string? userCursor;
    private volatile string? activeUserId;
    private volatile bool userLoading;
    private volatile bool userLoadingMore;
    private volatile ConfessionDto[] confessions = Array.Empty<ConfessionDto>();
    private volatile ConfessionDto[] userConfessions = Array.Empty<ConfessionDto>();
    public ConfessionDto[] UserConfessions => userConfessions;
    public bool UserLoading => userLoading;
    public bool HasMoreUserConfessions => userCursor is not null;
    public bool UserLoadingMore => userLoadingMore;
    private volatile string? cursor;
    private volatile bool loading;
    private volatile bool loadingMore;
    public KindKupoStore(AethernetSession session, KupoClient client)
    {
        this.session = session;
        this.client = client;
    }
    public void Refresh()
        {
            // 1. Initial refresh starts with cursor = null (fetch latest page)
            work.Run("confessions refresh", async token =>
            {
                var page = await client.FeedAsync(null, token);
                if (page is not null)
                {
                    confessions = page.Items;
                    cursor = page.NextCursor; // Save bookmark for next page
                }
            });
        }
    public void LoadMore()
        {
            // 2. If cursor is null, we're at the end — don't make unnecessary network requests
            if (cursor is null || loadingMore) return;

            loadingMore = true;
            work.Run("more", async token =>
            {
                var page = await client.FeedAsync(cursor, token); // Pass the bookmark
                if (page is not null)
                {
                    confessions = [..confessions, ..page.Items]; // Append new items to the list
                    cursor = page.NextCursor; // Update bookmark to next page
                }
            }, () => loadingMore = false);
        }
    public void ComposeConfession(string text, int expiryDays, Action<bool> onComplete)
    {
        work.Run("compose confession", async token =>
        {
            var created = await client.CreateConfessionAsync(text, expiryDays, token).ConfigureAwait(false);

            var newConfession = created ?? KindKupoMockData.CreateMockConfession(text, authorId: "me");

            confessions = [newConfession, ..confessions];
            userConfessions = [newConfession, ..userConfessions];

            return true;
        }, onComplete);
    }

     public void FetchUserConfessions(string userId)
    {
        activeUserId = userId;
        userConfessions = Array.Empty<ConfessionDto>();
        userCursor = null;
        userLoading = true;

        work.Run("user confessions refresh", async token =>
        {

            var page = await client.MyConfessionsAsync(userId, null, token).ConfigureAwait(false);


            if (activeUserId != userId) return;

            if (page is not null)
            {
                userConfessions = page.Items;
                userCursor = page.NextCursor;
            }
            else
            {
                // Mock fallback during local development
                userConfessions = KindKupoMockData.GetConfessions()
                    .Where(c => c.AuthorId == userId || userId == "me")
                    .ToArray();
            }
        }, () =>
        {
            if (activeUserId == userId) userLoading = false;
        });
    }
    public void SubmitResponse(string confessionId, string text, Action<bool> onComplete)
    {
        work.Run("submit response", async token =>
        {
            var created = await client.RespondAsync(confessionId, text, token).ConfigureAwait(false);
            var newResponse = created ?? KindKupoMockData.CreateMockResponse(confessionId, text, "me");

            // Find the confession in local memory and append the reply
            var target = confessions.FirstOrDefault(c => c.Id == confessionId)
                ?? KindKupoMockData.GetConfessions().FirstOrDefault(c => c.Id == confessionId);

            if (target is not null)
            {
                target.Responses.Add(newResponse);
            }

            return true;
        }, onComplete);
    }
    public void Dispose() => work.Dispose();
}
