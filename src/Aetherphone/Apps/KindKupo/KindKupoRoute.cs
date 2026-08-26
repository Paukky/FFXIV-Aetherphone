using Aetherphone.Core.Aethernet.Contracts;
using Aetherphone.Core.Social;

namespace Aetherphone.Apps.KindKupo;

internal enum KindKupoScreen
{
    Home,
    Inbox,
    Write,
    Respond,
    ResponseList,
    ComposeResponse
}
internal readonly record struct KindKupoRoute(
    KindKupoScreen Screen,
    string? UserId = null,
    ConfessionDto? Confession = null)
{
    public static readonly KindKupoRoute Home = new(KindKupoScreen.Home);
    public static readonly KindKupoRoute Inbox = new(KindKupoScreen.Inbox);
    public static readonly KindKupoRoute Write = new(KindKupoScreen.Write);
    public static readonly KindKupoRoute Respond = new(KindKupoScreen.Respond);
    public static KindKupoRoute ViewResponse(ConfessionDto confession) =>
        new(KindKupoScreen.ResponseList, Confession: confession);
    // public static KindKupoRoute UserInbox(string userId) =>
    //     new (KindKupoScreen.Inbox, UserId: userId);
    public static KindKupoRoute ComposeResponse(ConfessionDto confession) =>
        new(KindKupoScreen.ComposeResponse, Confession: confession);
}
