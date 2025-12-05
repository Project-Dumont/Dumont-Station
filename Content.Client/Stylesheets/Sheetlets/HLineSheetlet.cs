using Content.Client.UserInterface.Controls;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using static Content.Client.Stylesheets.StylesheetHelpers;

//Gabystation change
using Content.Client._Gabystation.Stylesheets;
using Content.Client._Gabystation.Stylesheets.Palette;

namespace Content.Client.Stylesheets.Sheetlets;

[CommonSheetlet]
public sealed class HLineSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        return
        [
            E<HLine>()
                .Class(StyleClass.Positive)
                .Panel(new StyleBoxFlat(sheet.PositivePalette.Text)),
            E<HLine>()
                .Class(StyleClass.Highlight)
                .Panel(new StyleBoxFlat(sheet.HighlightPalette.Text)),
            E<HLine>()
                .Class(StyleClass.Negative)
                .Panel(new StyleBoxFlat(sheet.NegativePalette.Text)),
            E<HLine>() //Gabystation change
                .Class(GabyStyleClass.GabyTheme)
                .Panel(new StyleBoxFlat(GabyPalettes.Gaby.Text)),
        ];
    }
}
