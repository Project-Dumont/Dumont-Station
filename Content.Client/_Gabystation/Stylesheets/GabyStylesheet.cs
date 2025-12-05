using System.Linq;
using Content.Client._Gabystation.Stylesheets.Palette;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Stylesheets;
using Content.Client.Stylesheets.Palette;
using Robust.Client.UserInterface;
using Content.Client.Stylesheets.SheetletConfigs;

namespace Content.Client._Gabystation.Stylesheets;

[Virtual]
public partial class GabystationStylesheet : NanotrasenStylesheet
{
    public override string StylesheetName => "Gabystation";

    //Colors
    public override ColorPalette PrimaryPalette => GabyPalettes.Gaby;
    public override ColorPalette HighlightPalette => GabyPalettes.Gaby;

    public GabystationStylesheet(object config, StylesheetManager man) : base(config, man)
    {
        var rules = new[]
        {
            GetAllSheetletRules<PalettedStylesheet, CommonSheetletAttribute>(man),
            GetAllSheetletRules<GabystationStylesheet, CommonSheetletAttribute>(man),
        };

        Stylesheet = new Stylesheet(rules.SelectMany(x => x).ToArray());
    }

}