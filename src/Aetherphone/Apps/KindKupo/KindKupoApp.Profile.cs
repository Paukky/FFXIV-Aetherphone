using Aetherphone.Core;
using Aetherphone.Core.Aethernet.Contracts;
using Aetherphone.Core.Apps;
using Aetherphone.Core.Localization;
using Aetherphone.Core.Onboarding;
using Aetherphone.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;


namespace Aetherphone.Apps.KindKupo;

internal sealed partial class KindKupoApp
{
    private void DrawHomeTopBar(Rect area)
    {
        var scale = UiScale.Current;
        var actions = new HeaderActions(area, scale, 2);

        var backHitMin = new Vector2(area.Min.X, area.Min.Y);
        var backHitMax = new Vector2(area.Min.X + 44f * scale, area.Min.Y + AppHeader.Height * scale);
        var backHovered = UiInteract.Hover(backHitMin, backHitMax);
        var backCenter = new Vector2(area.Min.X + 13f * scale, actions.RowCenterY);
        UiAnchors.Report("kindkupo.home.back", new Rect(backHitMin, backHitMax));
        if (BackButton.Draw("kindkupo.home.back", backCenter, 15f * scale, theme.Accent, backHovered, scale))
        {
            navigation.Back();
        }

        var rightReserve = area.Max.X - actions.TitleLimit;

        AppHeader.DrawTitleWithReserve(area, "kindkupo.home.title", DisplayName, rightReserve,
            AppPalettes.KindKupo.TitleInk, scale, new TextStyle(1.15f, FontWeight.SemiBold));

        UiAnchors.Report("kindkupo.rules", actions.Bounds(0));
        if (ui.IconButton(actions.Slot(0), actions.Radius, FontAwesomeIcon.QuestionCircle.ToIconString(),
                AppPalettes.KindKupo.MutedInk, AppSkin.Transparent, 1.1f, Loc.T(L.Conduct.Eyebrow),
                HoverLabelSide.Below))
        {
            conduct.ShowRules(Id);
        }

        UiAnchors.Report("kindkupo.inbox", actions.Bounds(1));
        if (ui.IconButton(actions.Slot(1), actions.Radius, FontAwesomeIcon.Inbox.ToIconString(),
                AppPalettes.KindKupo.Accent, AppSkin.Transparent, 1.1f, Loc.T(L.KindKupo.Inbox),
                HoverLabelSide.Below))
        {
            router.Push(KindKupoRoute.Inbox);
        }
    }

    private void DrawInbox(Rect area, string userId)
    {
        social.MarkSeen(Id);
        var scale = UiScale.Current;
        var padding = 16f * scale;
        var context = new PhoneContext(area, theme, navigation);
        AppHeader.Draw(context, Loc.T(L.KindKupo.YourInbox), back);
        var top = area.Min.Y + AppHeader.Height * scale + 8f * scale;
        var body = new Rect(new Vector2(area.Min.X + padding, top), new Vector2(area.Max.X - padding, area.Max.Y));
        using (AppSurface.Begin(body))
        {
            ImGui.Dummy(new Vector2(0f, 4f * scale));
            //var confessions = store.UserConfessions;
            var confessions = KindKupoMockData.GetInbox().KupoInboxes;
            foreach (var confession in confessions)
            {
                DrawConfessionCard(confession);
            }
        }
    }

    private void DrawResponseListScreen(Rect area, ConfessionDto confession)
    {
        var scale = UiScale.Current;
        var context = new PhoneContext(area, theme, navigation);
        AppHeader.Draw(context, Loc.T(L.KindKupo.Responses), back);

        var top = area.Min.Y + AppHeader.Height * scale + 8f * scale;
        var padding = 16f * scale;
        var body = new Rect(new Vector2(area.Min.X + padding, top), new Vector2(area.Max.X - padding, area.Max.Y));

        using (AppSurface.Begin(body))
        {

            DrawConfessionCard(confession);

            ImGui.Dummy(new Vector2(0f, 8f * scale));
            ui.SectionHeading($"{Loc.T(L.KindKupo.Replies)} ({confession.Responses.Count})");


            foreach (var response in confession.Responses)
            {
                DrawResponseCard(response);
            }

        }
    }
}
