using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

/// <summary>
/// UI control for the "Simple Wires" module.
/// Wires are buttons because IDK how to add the wires UI to this...
/// </summary>
public sealed class WiresModuleControl : BaseModuleControl
{
    private readonly BoxContainer _wiresContainer;

    private static readonly Dictionary<WireColor, Color> WireColorMap = new()
    {
        { WireColor.Red, Color.FromHex("#ff3333") },
        { WireColor.Blue, Color.FromHex("#3366ff") },
        { WireColor.Yellow, Color.FromHex("#ffdd33") },
        { WireColor.White, Color.FromHex("#eeeeee") },
        { WireColor.Black, Color.FromHex("#444444") },
    };

    private static readonly Dictionary<WireColor, string> WireColorNames = new()
    {
        { WireColor.Red, "RED" },
        { WireColor.Blue, "BLUE" },
        { WireColor.Yellow, "YELLOW" },
        { WireColor.White, "WHITE" },
        { WireColor.Black, "BLACK" },
    };

    private readonly List<Button> _wireButtons = new();

    public WiresModuleControl()
    {
        SetHeaderText(Loc.GetString("bomb-defusal-module-wires"));

        _wiresContainer = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };
        ContentContainer.AddChild(_wiresContainer);
    }

    public override void UpdateModuleState(BombDefusalModuleState state)
    {
        if (state is not WiresModuleState wiresState)
            return;

        if (_wireButtons.Count == 0)
        {
            _wiresContainer.RemoveAllChildren();
            for (var i = 0; i < wiresState.WireColors.Count; i++)
            {
                var wireIndex = i;
                var wireColor = wiresState.WireColors[i];
                var colorName = WireColorNames.GetValueOrDefault(wireColor, "???");

                // Wire row
                var wireRow = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                    HorizontalExpand = true,
                    Margin = new Thickness(0, 1),
                };

                // Wire index label
                var indexLabel = new Label
                {
                    Text = $"{wireIndex + 1}.",
                    FontColorOverride = Color.FromHex("#666688"),
                    MinWidth = 20,
                    Margin = new Thickness(0, 0, 4, 0),
                };
                wireRow.AddChild(indexLabel);

                // Wire button
                var wireButton = new Button
                {
                    HorizontalExpand = true,
                    MinHeight = 28,
                };

                var idx = wireIndex;
                wireButton.OnPressed += _ => RaiseAction(new CutWireAction(idx));

                wireRow.AddChild(wireButton);
                _wiresContainer.AddChild(wireRow);
                _wireButtons.Add(wireButton);
            }
        }

        for (var i = 0; i < wiresState.WireColors.Count; i++)
        {
            var wireColor = wiresState.WireColors[i];
            var isCut = wiresState.CutWires.Contains(i);
            var colorName = WireColorNames.GetValueOrDefault(wireColor, "???");
            var button = _wireButtons[i];

            button.Text = isCut
                ? $"── ✂ ── {colorName} (CUT)"
                : $"━━━━━━━━ {colorName}";
            button.Disabled = isCut || wiresState.IsSolved;

            if (isCut)
            {
                button.ModulateSelfOverride = Color.FromHex("#333333");
            }
            else
            {
                button.ModulateSelfOverride = WireColorMap.GetValueOrDefault(wireColor, Color.White);
            }
        }
    }
}
