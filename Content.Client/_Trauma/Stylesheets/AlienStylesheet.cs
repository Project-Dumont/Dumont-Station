// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Robust.Client.UserInterface.StylesheetHelpers;

namespace Content.Client._Trauma.Stylesheets;
public static class AlienStylesheet
{
    public const string ClassHighlight = "highlight";
    public const string ClassNegative = "negative";

    public static Stylesheet Create(IStylesheetManager man, IResourceCache cache)
    {
        var bgColor      = new Color(37, 31, 27);
        var textColor    = new Color(0, 255, 0);
        var borderColor  = new Color(212, 0, 98);
        var buttonBg     = new Color(60, 165, 68);
        var buttonBorder = new Color(55, 142, 64);
        var hoverColor   = new Color(50, 128, 108);
        var pressedColor = borderColor;
        var warningColor = new Color(1f, 0.65f, 0f);

        var asciiBorderBox = new StyleBoxFlat
        {
            BackgroundColor = bgColor,
            BorderColor = borderColor,
            BorderThickness = new Thickness(3f)
        };

        var buttonBox = new StyleBoxFlat
        {
            BackgroundColor = buttonBg,
            BorderColor = buttonBorder,
            BorderThickness = new Thickness(2f),
            Padding = new Thickness(8f, 4f)
        };

        var hoverBox = new StyleBoxFlat
        {
            BackgroundColor = hoverColor,
            BorderColor = buttonBorder,
            BorderThickness = new Thickness(2f),
            Padding = new Thickness(8f, 4f)
        };

        var pressedBox = new StyleBoxFlat
        {
            BackgroundColor = pressedColor,
            BorderColor = textColor,
            BorderThickness = new Thickness(2f),
            Padding = new Thickness(8f, 4f)
        };

        var rules = new StyleRule[]
        {
            // Window background panel
            Element<PanelContainer>().Class(StyleBase.ClassAngleRect)
                .Prop(PanelContainer.StylePropertyPanel, asciiBorderBox),

            // Window title bar
            Element<Label>().Class("FancyWindowTitle") // hardcoded award
                .Prop(Label.StylePropertyAlignMode, Label.AlignMode.Center)
                .Prop(Label.StylePropertyFontColor, textColor)
                .Prop(Label.StylePropertyFont, cache.NotoStack(size: 16)),

            // Other panels
            Element<PanelContainer>()
                .Prop(PanelContainer.StylePropertyPanel, asciiBorderBox),

            // All Labels
            Element<Label>()
                .Prop(Label.StylePropertyFontColor, textColor)
                .Prop(Label.StylePropertyFont, cache.NotoStack(size: 13)),

            // Buttons
            Child()
                .Parent(Element<ContainerButton>().Pseudo(ContainerButton.StylePseudoClassNormal))
                .Child(Element<PanelContainer>())
                .Prop(PanelContainer.StylePropertyPanel, buttonBox),

            // Button hover
            Child()
                .Parent(Element<ContainerButton>().Pseudo(ContainerButton.StylePseudoClassHover))
                .Child(Element<PanelContainer>())
                .Prop(PanelContainer.StylePropertyPanel, hoverBox),

            Child()
                .Parent(Element<ContainerButton>().Class(ClassHighlight))
                .Child(Element<PanelContainer>())
                .Prop(PanelContainer.StylePropertyPanel, hoverBox),

            // Button pressed
            Child()
                .Parent(Element<ContainerButton>().Pseudo(ContainerButton.StylePseudoClassPressed))
                .Child(Element<PanelContainer>())
                .Prop(PanelContainer.StylePropertyPanel, pressedBox),

            // Caution labels
            Element<Label>().Class(ClassNegative)
                .Prop(Label.StylePropertyFontColor, warningColor),
        };

        return new Stylesheet(man.SheetNano.Rules.Concat(rules).ToArray());
    }
}
