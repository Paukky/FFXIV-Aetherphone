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

    private volatile ConfessionDto[] confessions = Array.Empty<ConfessionDto>();
    private volatile ConfessionDto[] userConfession = Array.Empty<ConfessionDto>();

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
    public void Compose(string text, int expiryDays, Action<bool> onComplete)
        {
            work.Run("compose", async token =>
            {
                var created = await client.CreateConfessionAsync(text, expiryDays, token).ConfigureAwait(false);
                return created is not null;
            }, onComplete);
        }

    public void Dispose() => work.Dispose();
}
