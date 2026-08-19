using Aetherphone.Core.Social;

namespace Aetherphone.Apps.KindKupo;

internal enum KindKupoScreen
{
    Home,
    Inbox,
    UserInbox,
    User,
    Write,
    Respond
}
internal readonly record struct KindKupoRoute(
    KindKupoScreen Screen,
    string? UserId = null,
    string? PostId = null)
{
    public static readonly KindKupoRoute Home = new(KindKupoScreen.Home);
    public static readonly KindKupoRoute Inbox = new(KindKupoScreen.Inbox);
    public static readonly KindKupoRoute Write = new(KindKupoScreen.Write);
    public static readonly KindKupoRoute Respond = new(KindKupoScreen.Respond);

    public static KindKupoRoute User(string userId) => new(KindKupoScreen.User, userId);
    public static KindKupoRoute UserInbox(string sourceId) =>
        new(KindKupoScreen.UserInbox, sourceId);
}
