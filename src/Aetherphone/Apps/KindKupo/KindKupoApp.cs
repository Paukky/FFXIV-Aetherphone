using Aetherphone.Core;
using Aetherphone.Core.Aethernet;
using Aetherphone.Core.Aethernet.Contracts;
using Aetherphone.Core.Animation;
using Aetherphone.Core.Apps;
using Aetherphone.Core.Conduct;
using Aetherphone.Core.Confirm;
using Aetherphone.Core.Home;
using Aetherphone.Core.Localization;
using Aetherphone.Core.Lodestone;
using Aetherphone.Core.Net;
using Aetherphone.Core.Notifications;
using Aetherphone.Core.Onboarding;
using Aetherphone.Core.Report;
using Aetherphone.Core.Theme;
using Aetherphone.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;


namespace Aetherphone.Apps.KindKupo;

internal sealed partial class KindKupoApp : IPhoneApp
{
    public string Id => "kindkupo";
    public string DisplayName => Loc.T(L.Apps.KindKupo);
    public string Glyph => "KK";
    public int BadgeCount => social.UnseenCount(Id);
    private readonly int writtenCount;
    private readonly int responseCount;
    private readonly int kudosCount;

    private readonly KindKupoStore store;
    private readonly ViewRouter<KindKupoRoute> router;
    private readonly RouterDraw<KindKupoRoute> drawView;
    private readonly AppSkin ui = new(AppPalettes.KindKupo);
    private readonly AethernetSession session;
    private readonly AethernetApi net;
    private readonly ConductGateService conduct;
    private readonly SocialNotificationService social;
    RichTextLayout? bodyLayout = null;
    private PhoneTheme theme = PhoneTheme.Default;
    private string draft = string.Empty;
    private INavigator navigation = null!;
    private readonly Action back;
    public KindKupoApp(AethernetSession session, AethernetApi net, ConfirmService confirm,
        ReportService report, ConductGateService conduct, SocialNotificationService social)
    {
        this.session = session;
        this.net = net;
        this.conduct = conduct;
        this.social = social;
        store = new KindKupoStore(session, net.Kupo);

        drawView = DrawView;
        router = new ViewRouter<KindKupoRoute>(KindKupoRoute.Home);
        back = () => router.Pop();
    }
    public void Dispose() => store.Dispose();
    public void OnOpened()
    {
        router.Reset();
        draft = string.Empty;
        conduct.NotifyAppOpened(Id);
    }

    public void OnClosed()
    {
        router.Reset();
        draft = string.Empty;
    }

    public void Draw(in PhoneContext context)
    {
        theme = context.Theme;
        navigation = context.Navigation;
        ui.Theme = theme;
        router.Draw(context.Content, AppSkin.Transparent, ImGui.GetIO().DeltaTime, drawView);
    }

    private void DrawView(KindKupoRoute route, Rect area, int depth)
    {
        ui.Body(area);
        switch (route.Screen)
        {
            case KindKupoScreen.Write:
                DrawWriteScreen(area);
                break;
            case KindKupoScreen.Inbox:
                DrawInbox(area, session.CurrentUser.Id);
                break;
            case KindKupoScreen.Respond:
                DrawResponseFeed(area);
                break;
            case KindKupoScreen.ResponseList:
                if (route.Confession is not null)
                {
                    DrawResponseListScreen(area, route.Confession);
                }
                break;
            case KindKupoScreen.ComposeResponse:
                if (route.Confession is not null)
                {
                    DrawComposeResponse(area, route.Confession);
                }
                break;
            default:
                DrawHome(area);
                break;
        }

    }

    private void DrawConfessionCard(ConfessionDto confession)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorScreenPos();
        var pad = 14f * scale;
        var width = ImGui.GetContentRegionAvail().X;
        var rounding = 16f * scale;
        var cardGap = 12f * scale;

        var contentLeft = origin.X + pad;
        var contentRight = origin.X + width - pad;
        var contentWidth = MathF.Max(1f, contentRight - contentLeft);
        var textTop = origin.Y + pad;

        var textHeight = confession.Text.Length == 0
            ? 0f
            : (bodyLayout?.Size.Y ?? Typography.MeasureWrapped(confession.Text, contentWidth, 1.05f));

        var textToFooterGap = 10f * scale;
        var footerHeight = 24f * scale;
        var footerCenterY = textTop + textHeight + textToFooterGap + footerHeight * 0.5f;

        var cardHeight = pad + textHeight + textToFooterGap + footerHeight + pad;
        var cardBottom = origin.Y + cardHeight;

        ui.Card(drawList, origin, new Vector2(origin.X + width, cardBottom), rounding);

        if (confession.Text.Length > 0 && bodyLayout is null)
        {
            ImGui.SetCursorScreenPos(new Vector2(contentLeft, textTop));
            using (Typography.WrapAt(contentRight))
            using (Plugin.Fonts.Push(1.05f))
            using (ImRaii.PushColor(ImGuiCol.Text, AppPalettes.KindKupo.BodyInk))
            {
                Typography.Wrapped(confession.Text);
            }
        }

        DrawCardFooter(confession, contentLeft, contentWidth, footerCenterY);

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, cardHeight + cardGap));
    }

    private void DrawCardFooter(ConfessionDto confession, float left, float width, float centerY)
    {

        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var right = left + width;


        var stamp = TimeText.Ago(confession.CreatedAt);
        var stampSize = Typography.Measure(stamp, 0.85f, FontWeight.Regular);
        var stampPos = new Vector2(left, centerY - stampSize.Y * 0.5f);
        Typography.Draw(drawList, stampPos, stamp, AppPalettes.KindKupo.MutedInk, 0.85f, FontWeight.Regular);

        var iconWidth = 16f * scale;
        var iconCenter = new Vector2(right - iconWidth * 0.8f, centerY);
        if (router.Current == KindKupoRoute.Respond)
        {
            if (ui.IconButton(iconCenter, 16f * scale, FontAwesomeIcon.Pen.ToIconString(),
                    AppPalettes.KindKupo.MutedInk,
                    new Vector4(0f, 0f, 0f, 0f), 1f, Loc.T(L.KindKupo.Respond)))
            {
                router.Push(KindKupoRoute.ComposeResponse(confession));
            }
        }

        if (router.Current == KindKupoRoute.Inbox)
        {
            if (ui.IconButton(iconCenter, 16f * scale, FontAwesomeIcon.CommentDots.ToIconString(),
                    AppPalettes.KindKupo.MutedInk,
                    new Vector4(0f, 0f, 0f, 0f), 1f, Loc.T(L.KindKupo.ViewReplies, confession.Responses.Count)))
            {
                router.Push(KindKupoRoute.ViewResponse(confession));
            }
        }
        //AppSkin.Icon(drawList, iconCenter, icon, AppPalettes.KindKupo.MutedInk, iconScale);
    }

    private void DrawResponseCard(ResponseDto response)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var origin = ImGui.GetCursorScreenPos();
        var pad = 14f * scale;
        var width = ImGui.GetContentRegionAvail().X;
        var rounding = 16f * scale;
        var cardGap = 12f * scale;

        var contentLeft = origin.X + pad;
        var contentRight = origin.X + width - pad;
        var contentWidth = MathF.Max(1f, contentRight - contentLeft);
        var textTop = origin.Y + pad;

        var textHeight = response.Text.Length == 0
            ? 0f
            : (bodyLayout?.Size.Y ?? Typography.MeasureWrapped(response.Text, contentWidth, 1.05f));
        var textToFooterGap = 10f * scale;
        var footerHeight = 24f * scale;

        var cardHeight = pad + textHeight + textToFooterGap + footerHeight + pad;
        var cardBottom = origin.Y + cardHeight;
        ui.Card(drawList, origin, new Vector2(origin.X + width, cardBottom), rounding);


        if (response.Text.Length > 0 && bodyLayout is null)
        {
            ImGui.SetCursorScreenPos(new Vector2(contentLeft, textTop));
            using (Typography.WrapAt(contentRight))
            using (Plugin.Fonts.Push(1.05f))
            using (ImRaii.PushColor(ImGuiCol.Text, AppPalettes.KindKupo.BodyInk))
            {
                Typography.Wrapped(response.Text);
            }
        }
        var stamp = TimeText.Ago(response.CreatedAt);
        var stampSize = Typography.Measure(stamp, 0.85f, FontWeight.Regular);
        var stampPos = new Vector2(contentLeft, cardBottom - pad - stampSize.Y * 0.5f);
        Typography.Draw(drawList, stampPos, stamp, AppPalettes.KindKupo.MutedInk, 0.85f, FontWeight.Regular);

        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(width, cardHeight + cardGap));
    }


    private void DrawHome(Rect area)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var padding = 16f * scale;
        DrawHomeTopBar(area);

        var contentMinY = area.Min.Y + AppHeader.Height * scale;
        var availableHeight = area.Max.Y - contentMinY;


        var statCardHeight = 80f * scale;
        var buttonHeight = 42f * scale;
        var gapStatToBtn = 20f * scale;
        var gapBetweenBtns = 12f * scale;
        var totalBlockHeight = statCardHeight + gapStatToBtn + buttonHeight + gapBetweenBtns + buttonHeight;

        var top = contentMinY + MathF.Max(0f, (availableHeight - totalBlockHeight) * 0.5f);
        var width = area.Width - padding * 2f;


        var statMin = new Vector2(area.Min.X + padding, top);
        var statMax = new Vector2(statMin.X + width, top + statCardHeight);

        UiAnchors.Report("kindkupo.stats", new Rect(statMin, statMax));
        ui.Card(drawList, statMin, statMax, 12f * scale, elevated: false);

        var colWidth = width / 3f;


        DrawStatColumn(drawList, new Rect(statMin, new Vector2(statMin.X + colWidth, statMax.Y)),
            writtenCount.ToString(), Loc.T(L.KindKupo.Written), scale);


        var div1X = statMin.X + colWidth;
        drawList.AddLine(new Vector2(div1X, statMin.Y + 12f * scale), new Vector2(div1X, statMax.Y - 12f * scale),
            ImGui.GetColorU32(AppPalettes.KindKupo.CardStroke));


        DrawStatColumn(drawList, new Rect(new Vector2(div1X, statMin.Y), new Vector2(div1X + colWidth, statMax.Y)),
            responseCount.ToString(), Loc.T(L.KindKupo.Responses), scale);


        var div2X = div1X + colWidth;
        drawList.AddLine(new Vector2(div2X, statMin.Y + 12f * scale), new Vector2(div2X, statMax.Y - 12f * scale),
            ImGui.GetColorU32(AppPalettes.KindKupo.CardStroke));


        DrawStatColumn(drawList, new Rect(new Vector2(div2X, statMin.Y), statMax),
            kudosCount.ToString(), Loc.T(L.KindKupo.Kudos), scale);


        var buttonY = statMax.Y + gapStatToBtn;


        var writeRect = new Rect(new Vector2(area.Min.X + padding, buttonY),
            new Vector2(area.Max.X - padding, buttonY + buttonHeight));

        UiAnchors.Report("kindkupo.write", writeRect);
        if (ui.PillButton(writeRect, Loc.T(L.KindKupo.Write), filled: true))
        {
            router.Push(KindKupoRoute.Write);
        }


        var respondY = buttonY + buttonHeight + gapBetweenBtns;
        var respondRect = new Rect(new Vector2(area.Min.X + padding, respondY),
            new Vector2(area.Max.X - padding, respondY + buttonHeight));

        UiAnchors.Report("kindkupo.respond", respondRect);
        if (ui.PillButton(respondRect, Loc.T(L.KindKupo.Respond), filled: false))
        {
            router.Push(KindKupoRoute.Respond);
        }
    }

    private static void DrawStatColumn(ImDrawListPtr drawList, Rect rect, string value, string label, float scale)
    {
        var numberPos = new Vector2(rect.Center.X, rect.Min.Y + 20f * scale);
        var labelPos = new Vector2(rect.Center.X, rect.Min.Y + 48f * scale);

        Typography.DrawCentered(drawList, numberPos, value, AppPalettes.KindKupo.TitleInk, 1.4f, FontWeight.Bold);
        Typography.DrawCentered(drawList, labelPos, label, AppPalettes.KindKupo.MutedInk, 0.85f, FontWeight.Medium);
    }
}
