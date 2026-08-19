using Aetherphone.Core;
using Aetherphone.Core.Aethernet;
using Aetherphone.Core.Animation;
using Aetherphone.Core.Apps;
using Aetherphone.Core.Conduct;
using Aetherphone.Core.Confirm;
using Aetherphone.Core.Home;
using Aetherphone.Core.Localization;
using Aetherphone.Core.Lodestone;
using Aetherphone.Core.Net;
using Aetherphone.Core.Notifications;
using Aetherphone.Core.Report;
using Aetherphone.Core.Theme;
using Aetherphone.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

namespace Aetherphone.Apps.KindKupo;

internal sealed partial class KindKupoApp
{
    private void DrawHomeTopBar(Rect area)
    {

        var scale = UiScale.Current;
        var rowCenterY = area.Min.Y + AppHeader.Height * scale * 0.5f;
        const float titleScale = 1.3f;
        var leftReserve = area.Min.X + 84f * scale;
        var rightReserve = area.Max.X - 112f * scale;
        var titlePadding = new Vector2(12f * scale, 6f * scale);
        var titleCenter = new Vector2(area.Center.X, rowCenterY);
        var titleSize = Typography.Measure(DisplayName, titleScale, FontWeight.Bold);
        var titleMin = titleCenter - titleSize * 0.5f - titlePadding;
        var titleMax = titleCenter + titleSize * 0.5f + titlePadding;
        UiInteract.HoverHighlight(ImGui.GetWindowDrawList(), titleMin, titleMax, (titleMax.Y - titleMin.Y) * 0.5f);
        Typography.DrawCentered(titleCenter, DisplayName, AppPalettes.KindKupo.TitleInk, titleScale, FontWeight.Bold);

        var rulesCenter = new Vector2(area.Max.X - 24f * scale, rowCenterY);
        var profileCenter = new Vector2(area.Max.X - 48f * scale, rowCenterY);
        if (ui.IconButton(rulesCenter, 16f * scale, FontAwesomeIcon.QuestionCircle.ToIconString(),
                AppPalettes.KindKupo.MutedInk, AppSkin.Transparent, 1.1f, Loc.T(L.Conduct.Eyebrow),
                HoverLabelSide.Below))
        {
            conduct.ShowRules(Id);
        }

        if (ui.IconButton(profileCenter, 16f * scale, FontAwesomeIcon.Inbox.ToIconString(),
                AppPalettes.KindKupo.Accent, AppSkin.Transparent, 1.1f,
                Loc.T(L.Conduct.Eyebrow), HoverLabelSide.Below))
        {
            router.Push(KindKupoRoute.Inbox);
        }
    }

    private void DrawProfile(Rect area)
    {
        if (session.CurrentUser is not { } me)
        {
            Typography.DrawCentered(area.Center, "Loading profile...", theme.TextMuted, 1.0f);
            return;
        }

        var scale = ImGuiHelpers.GlobalScale;
        var context = new PhoneContext(area, theme, navigation);

        string characterName = Initials.Of(me.Name);
    }


}
