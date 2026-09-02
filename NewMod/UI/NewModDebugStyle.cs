using UnityEngine;
using S = ReactUI.Style;

namespace NewMod.UI;

public static class NewModDebugStyles
{
    private static float Scale { get; set; } = 1f;

    public static void Register()
    {
        Scale = Mathf.Clamp(Screen.height / 1080f, 0.7f, 1.6f);

        var sheet = new S.StyleSheet();

        sheet[".nm-debug-panel"] = new S.Style
        {
            Position = S.PositionType.Absolute,
            Inset = new S.EdgeValues(Px(20f), Px(20f), Px(20f), float.NaN),
            Width = S.StyleValue.Percent(46f),
            MaxWidth = S.StyleValue.Px(Px(760f)),
            MinWidth = S.StyleValue.Px(Px(540f)),
            Background = "#08080A",
            BorderWidth = Px(3f),
            BorderColor = "#FFFFFF",
            BorderRadius = Px(10f),
            Overflow = S.Overflow.Hidden
        };

        sheet[".nm-debug-header"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            AlignItems = S.AlignItems.Center,
            JustifyContent = S.JustifyContent.SpaceBetween,
            Padding = Edge(12f, 14f),
            Background = "#08080A",
            BorderWidth = Px(2f),
            BorderColor = "rgba(255,255,255,0.35)"
        };

        sheet[".nm-debug-brand"] = new S.Style { FlexDirection = S.FlexDirection.Row, AlignItems = S.AlignItems.Center, Gap = Px(7f) };

        sheet[".nm-debug-title"] = new S.Style { FontSize = Px(21f), FontWeight = 800, Color = "#FFFFFF" };

        sheet[".nm-debug-title-muted"] = new S.Style { FontSize = Px(21f), FontWeight = 800, Color = "#9A9AA6" };

        sheet[".nm-debug-tabs"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            Padding = Edge(8f, 10f),
            Gap = Px(6f),
            Background = "#17171C",
            BorderWidth = Px(2f),
            BorderColor = "rgba(255,255,255,0.35)"
        };

        sheet[".nm-debug-tab"] = new S.Style
        {
            FlexGrow = 1f,
            Padding = Edge(7f, 8f),
            BorderRadius = Px(4f),
            BorderWidth = Px(2f),
            BorderColor = "rgba(255,255,255,0.35)",
            Background = "#000000",
            Color = "#B8B8C2",
            FontSize = Px(12f),
            FontWeight = 700,
            Cursor = S.CursorType.Pointer,
            Hover = new S.Style { BorderColor = "#FFFFFF", Color = "#FFFFFF" }
        };

        sheet[".nm-debug-tab-active"] = new S.Style { Background = "#7D42A8", BorderColor = "#FFFFFF", Color = "#FFFFFF" };

        sheet[".nm-debug-scroll"] = new S.Style { FlexGrow = 1f, Padding = Edge(10f), Background = "#08080A" };

        sheet[".nm-debug-stack"] = new S.Style { Gap = Px(9f) };

        sheet[".nm-debug-row"] = new S.Style { FlexDirection = S.FlexDirection.Row, AlignItems = S.AlignItems.Center, Gap = Px(8f) };

        sheet[".nm-debug-row-wrap"] = new S.Style { FlexDirection = S.FlexDirection.Row, FlexWrap = S.FlexWrap.Wrap, AlignItems = S.AlignItems.Center, Gap = Px(7f) };

        sheet[".nm-debug-section"] = new S.Style
        {
            Gap = Px(7f),
            Padding = Edge(10f, 12f),
            Background = "#17171C",
            BorderWidth = Px(2f),
            BorderColor = "rgba(255,255,255,0.35)",
            BorderRadius = Px(6f)
        };

        sheet[".nm-debug-section-title"] = new S.Style { FontSize = Px(13f), FontWeight = 800, Color = "#B8B8C2" };

        sheet[".nm-debug-label"] = new S.Style
        {
            Width = S.StyleValue.Px(Px(145f)),
            FlexShrink = 0f,
            FontSize = Px(13f),
            FontWeight = 700,
            Color = "#9A9AA6"
        };

        sheet[".nm-debug-value"] = new S.Style { FlexGrow = 1f, FontSize = Px(13f), Color = "#FFFFFF" };

        sheet[".nm-debug-muted"] = new S.Style { FontSize = Px(13f), Color = "#9A9AA6" };

        sheet[".nm-debug-button"] = new S.Style
        {
            Padding = Edge(7f, 11f),
            BorderRadius = Px(4f),
            BorderWidth = Px(2f),
            BorderColor = "#FFFFFF",
            Background = "#000000",
            Color = "#FFFFFF",
            FontSize = Px(13f),
            FontWeight = 700,
            Cursor = S.CursorType.Pointer,
            Hover = new S.Style { Background = "#25252B" }
        };

        sheet[".nm-debug-button-danger"] = new S.Style { Background = "#821F2A", BorderColor = "#FFFFFF", Hover = new S.Style { Background = "#A32937" } };

        sheet[".nm-debug-close"] = new S.Style
        {
            Width = S.StyleValue.Px(Px(34f)),
            Height = S.StyleValue.Px(Px(30f)),
            BorderRadius = Px(4f),
            BorderWidth = Px(2f),
            BorderColor = "#FFFFFF",
            Background = "#000000",
            Color = "#FFFFFF",
            FontSize = Px(17f),
            FontWeight = 800,
            Cursor = S.CursorType.Pointer,
            Hover = new S.Style { Background = "#25252B" }
        };

        sheet[".nm-debug-selector"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            AlignItems = S.AlignItems.Stretch,
            Height = S.StyleValue.Px(Px(38f)),
            BorderRadius = Px(5f),
            BorderWidth = Px(2f),
            BorderColor = "#FFFFFF",
            Background = "#000000",
            Overflow = S.Overflow.Hidden
        };

        sheet[".nm-debug-selector-button"] = new S.Style
        {
            Width = S.StyleValue.Px(Px(42f)),
            BorderWidth = Px(1f),
            BorderColor = "rgba(255,255,255,0.35)",
            Background = "#17171C",
            Color = "#FFFFFF",
            FontSize = Px(22f),
            FontWeight = 800,
            Cursor = S.CursorType.Pointer,
            Hover = new S.Style { Background = "#7D42A8" }
        };

        sheet[".nm-debug-selector-value"] = new S.Style
        {
            FlexGrow = 1f,
            AlignSelf = S.AlignSelf.Center,
            TextAlign = S.TextAlign.Center,
            FontSize = Px(14f),
            FontWeight = 700,
            Color = "#FFFFFF"
        };

        sheet[".nm-debug-toggle"] = new S.Style { Height = S.StyleValue.Px(Px(30f)), Color = "#7D42A8", Cursor = S.CursorType.Pointer };

        sheet[".nm-debug-slider"] = new S.Style { Height = S.StyleValue.Px(Px(30f)), FlexGrow = 1f, Color = "#7D42A8", Cursor = S.CursorType.Pointer };

        ReactUI.UI.RegisterStyles(sheet);
    }

    private static float Px(float value)
    {
        return Mathf.Round(value * Scale);
    }

    private static S.EdgeValues Edge(float all)
    {
        return new S.EdgeValues(Px(all));
    }

    private static S.EdgeValues Edge(float vertical, float horizontal)
    {
        return new S.EdgeValues(Px(vertical), Px(horizontal));
    }
}