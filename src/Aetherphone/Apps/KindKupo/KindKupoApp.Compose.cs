using Aetherphone.Core;
using Aetherphone.Core.Aethernet.Contracts;
using Aetherphone.Core.Apps;
using Aetherphone.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace Aetherphone.Apps.KindKupo;

internal enum KindKupoExpiryDate
{
    Never,
    OneDay,
    ThreeDays,
    OneWeek,
}

internal sealed partial class KindKupoApp
{

    private int activeExpiryTab;
    private readonly List<string> expiryTabLabels = new();
    private volatile bool composeBusy;

    private void DrawWriteScreen(Rect area)
    {
        var scale = UiScale.Current;
        var padding = 16f * scale;


        AppHeader.Draw(new PhoneContext(area, theme, navigation), "New Confession", back);

        var top = area.Min.Y + AppHeader.Height * scale + 10f * scale;


        var bottomReserved = 90f * scale;
        var availableHeight = area.Max.Y - top - bottomReserved;


        var fieldHeight = Math.Clamp(availableHeight * 0.70f, 80f * scale, 280f * scale);
        var fieldMin = new Vector2(area.Min.X + padding, top);
        var fieldMax = new Vector2(fieldMin.X + (area.Width - padding * 2f), top + fieldHeight);
        var fieldRect = new Rect(fieldMin, fieldMax);

        if (!router.IsTransitioning)
        {
            ui.Field(String.Empty, "##confessionText", ref draft, 1000, true, fieldHeight / scale);
        }
        else
        {
            ui.Card(ImGui.GetWindowDrawList(), fieldMin, fieldMax, 8f * scale, elevated: false);
        }



        DrawExpiryTabBar(fieldRect, scale);

        var buttonY = fieldMax.Y + 40f * scale;
        var buttonHeight = MathF.Min(42f * scale, MathF.Max(30f * scale, area.Max.Y - buttonY - 10f * scale));
        var buttonRect = new Rect(
            new Vector2(area.Min.X + padding, buttonY),
            new Vector2(area.Max.X - padding, buttonY + buttonHeight));

        bool canSubmit = !string.IsNullOrWhiteSpace(draft);
        if (AppSkin.PillButton(buttonRect, "Post Confession", filled: true, enabled: canSubmit, theme))
        {
            SubmitConfession(draft,activeExpiryTab);
            draft = string.Empty;
            router.Pop();

        }
    }

    private void DrawExpiryTabBar(Rect draftRect, float scale)
    {

        if (expiryTabLabels.Count == 0)
        {
            expiryTabLabels.Add("Never");
            expiryTabLabels.Add("1 Day");
            expiryTabLabels.Add("3 Days");
            expiryTabLabels.Add("1 Week");
        }

        var top = draftRect.Min.Y + AppHeader.Height * scale;

        var barRect = new Rect(new Vector2(draftRect.Min.X, top), new Vector2(draftRect.Max.X, top + 517.5f * scale));

        activeExpiryTab = SegmentStrip.Draw(
            "kindkupo.expiryTab",
            barRect,
            expiryTabLabels,
            activeExpiryTab,
            AppPalettes.KindKupo,
            trackHeight: 24f,
            textScale: 0.72f);
    }

    private void DrawResponseScreen(Rect area)
    {
        var scale = UiScale.Current;
        var padding = 16f * scale;
        var feed = KindKupoMockData.GetConfessions();
        AppHeader.Draw(new PhoneContext(area, theme, navigation), "Respond", back);

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
    private void SubmitConfession(string msg, int expirydate)
    {
        composeBusy = true;
        store.Compose(msg, expirydate, success => composeBusy = false);
    }
}
