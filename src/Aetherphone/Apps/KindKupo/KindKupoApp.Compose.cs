using Aetherphone.Core;
using Aetherphone.Core.Aethernet.Contracts;
using Aetherphone.Core.Apps;
using Aetherphone.Core.Localization;
using Aetherphone.Core.Onboarding;
using Aetherphone.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

namespace Aetherphone.Apps.KindKupo;


internal sealed partial class KindKupoApp
{
    private string replyDraft = string.Empty;
    private int activeExpiryTab;
    private volatile bool composeBusy;
    private static readonly List<LocString> expiryTabOptions =
    [
        L.KindKupo.ExpiryNever,
        L.KindKupo.Expiry1d,
        L.KindKupo.Expiry3d,
        L.KindKupo.Expiry7d,
    ];
    private readonly List<string> expiryTabLabels = new(expiryTabOptions.Count);
    private void DrawWriteScreen(Rect area)
        {
            var scale = UiScale.Current;
            var context = new PhoneContext(area, theme, navigation);
            var rowCenterY = area.Min.Y + AppHeader.Height * scale * 0.5f;
            var origin = ImGui.GetCursorScreenPos();

            var actionLabel = Loc.T(L.KindKupo.Post);
            var postWidth = AppSkin.HeaderActionWidth(actionLabel);
            var postLeft = area.Max.X - 12f * scale - postWidth;

            var maxPostLength = 1000;
            var stripWidth = 120f * scale;
            const float trackHeight = 25f;
            var stripHeight = trackHeight * scale;
            var stripGap = 8f * scale;
            var stripRight = postLeft - stripGap;
            var stripLeft = stripRight - stripWidth;
            var stripRect = new Rect(
                new Vector2(stripLeft, rowCenterY - stripHeight * 0.5f),
                new Vector2(stripRight, rowCenterY + stripHeight * 0.5f));


            var rightReserve = area.Max.X - stripLeft + 6f * scale;

            AppHeader.Draw(context, string.Empty, back);

            AppHeader.DrawTitleWithReserve(
                area,
                "kindkupo.new.confession",
                Loc.T(L.KindKupo.NewConfession),
                rightReserve,
                AppPalettes.KindKupo.TitleInk,
                scale,
                new TextStyle(1.05f, FontWeight.SemiBold));


            expiryTabLabels.Clear();
            for (var index = 0; index < expiryTabOptions.Count; index++)
            {
                expiryTabLabels.Add(Loc.T(expiryTabOptions[index]));
            }

            UiAnchors.Report("kindkupo.compose.expiry", stripRect);
            activeExpiryTab = SegmentStrip.Draw(
                "kindkupo.expiryTab",
                stripRect,
                expiryTabLabels,
                activeExpiryTab,
                AppPalettes.KindKupo,
                trackHeight: trackHeight,
                textScale: 0.70f);


              var canSubmit = !string.IsNullOrWhiteSpace(draft) && !composeBusy;
                if (ui.HeaderAction(area, Loc.T(L.KindKupo.Post), canSubmit))
                {
                    composeBusy = true;
                    store.ComposeConfession(draft, activeExpiryTab, success =>
                    {
                        composeBusy = false;
                        if (success)
                        {
                            draft = string.Empty;
                            router.Pop();
                        }
                    });
                }

            ui.Field(String.Empty, "##confessionText", ref draft, maxPostLength, true, area.Max.Y);
            //placeholder text
            if (draft.Length == 0)
            {
                var placeholderPos = new Vector2(
                    origin.X + 16f * scale,
                    origin.Y + 63f * scale);

                Typography.Draw(
                    placeholderPos,
                    Loc.T(L.KindKupo.Placeholder),
                    AppPalettes.KindKupo.MutedInk,
                    1.0f);
            }

            var remaining = maxPostLength - draft.Length;
            var counter = string.Format(Loc.Culture, "{0} / {1}", draft.Length, maxPostLength);
            var counterColor = remaining < 40
                ? (remaining < 0 ? theme.Danger : new Vector4(0.95f, 0.65f, 0.20f, 1f))
                : AppPalettes.KindKupo.MutedInk;
            var counterSize = Typography.Measure(counter, 1f, FontWeight.Medium);
            Typography.Draw(new Vector2(area.Max.X - 6f * scale - counterSize.X, (area.Max.Y - 44f * 0.5f * scale) - counterSize.Y * 0.5f),
                counter, counterColor, 1f, FontWeight.Medium);
        }

     private void DrawComposeResponse(Rect area, ConfessionDto confession)
    {
        var scale = UiScale.Current;
        var context = new PhoneContext(area, theme, navigation);
        var origin = ImGui.GetCursorScreenPos();
        var padding = 16f * scale;

        AppHeader.Draw(context, Loc.T(L.KindKupo.Respond), back);

        // Header "Post" button
        var canSubmit = !string.IsNullOrWhiteSpace(replyDraft) && !composeBusy;
        if (ui.HeaderAction(area, Loc.T(L.KindKupo.Post), canSubmit))
        {
            composeBusy = true;
            store.SubmitResponse(confession.Id, replyDraft, success =>
            {
                composeBusy = false;
                if (success)
                {
                    replyDraft = string.Empty;
                    router.Pop();
                }
            });
        }

        var top = area.Min.Y + AppHeader.Height * scale + 8f * scale;
        var body = new Rect(new Vector2(area.Min.X + padding, top), new Vector2(area.Max.X - padding, area.Max.Y));

        using (AppSurface.Begin(body))
        {

            DrawConfessionCard(confession);

            ImGui.Dummy(new Vector2(0f, 8f * scale));


            var availableHeight = MathF.Max(120f, (area.Max.Y - ImGui.GetCursorScreenPos().Y - 40f * scale) / scale);
            var fieldPos = ImGui.GetCursorScreenPos();
            ui.Field(string.Empty, "##replyDraft", ref replyDraft, 1000, true, availableHeight);


            if (replyDraft.Length == 0)
            {
                Typography.Draw(
                    new Vector2(fieldPos.X + 16f * scale, fieldPos.Y + 30f * scale),
                    Loc.T(L.KindKupo.ReplyPlaceholder),
                    AppPalettes.KindKupo.MutedInk,
                    1.0f);
            }
        }
    }
    private void DrawResponseFeed(Rect area)
    {
        var scale = UiScale.Current;
        var padding = 16f * scale;
        var feed = KindKupoMockData.GetConfessions();
        AppHeader.Draw(new PhoneContext(area, theme, navigation), Loc.T(L.KindKupo.Respond), back);

        var top = area.Min.Y + AppHeader.Height * scale + 8f * scale;
        var body = new Rect(new Vector2(area.Min.X + padding, top), new Vector2(area.Max.X - padding, area.Max.Y));
        using (AppSurface.Begin(body))
        {
            ImGui.Dummy(new Vector2(0f, 4f * scale));
            foreach (var confession in feed)
            {
                DrawConfessionCard(confession);
            }
        }
    }
}
