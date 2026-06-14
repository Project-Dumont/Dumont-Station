using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

/// <summary>
/// UI control for the "Simple Wires" module.
/// Displays colored wires that can be cut by clicking.
/// </summary>
public sealed class WiresModuleControl : BaseModuleControl
{
    private readonly Label _title;
    private readonly BoxContainer _wiresContainer;
    private readonly Label _solvedLabel;

    private static readonly Dictionary<WireColor, Color> WireColorMap = new()
    {
        { WireColor.Red, Color.FromHex("#ff3333") },
        { WireColor.Blue, Color.FromHex("#3366ff") },
        { WireColor.Yellow, Color.FromHex("#ffdd33") },
        { WireColor.White, Color.FromHex("#eeeeee") },
        { WireColor.Black, Color.FromHex("#333333") },
    };

    public WiresModuleControl()
    {
        _title = new Label
        {
            Text = Loc.GetString("bomb-defusal-module-wires"),
            FontColorOverride = Color.FromHex("#00ff41"),
            Margin = new Thickness(0, 0, 0, 4),
        };
        AddChild(_title);

        _wiresContainer = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };
        AddChild(_wiresContainer);

        _solvedLabel = new Label
        {
            Text = Loc.GetString("bomb-defusal-module-solved"),
            FontColorOverride = Color.FromHex("#00ff41"),
            Visible = false,
            Align = Label.AlignMode.Center,
        };
        AddChild(_solvedLabel);
    }

    public override void UpdateState(BombDefusalModuleState state)
    {
        if (state is not WiresModuleState wiresState)
            return;

        _solvedLabel.Visible = wiresState.IsSolved;

        _wiresContainer.RemoveAllChildren();

        for (var i = 0; i < wiresState.WireColors.Count; i++)
        {
            var wireIndex = i;
            var wireColor = wiresState.WireColors[i];
            var isCut = wiresState.CutWires.Contains(i);

            var wireButton = new Button
            {
                Text = isCut
                    ? $"── ✂ ── Wire {wireIndex + 1} (Cut)"
                    : $"━━━━━━ Wire {wireIndex + 1}",
                Disabled = isCut || wiresState.IsSolved,
                Margin = new Thickness(0, 1),
                HorizontalExpand = true,
            };

            wireButton.ModulateSelfOverride = WireColorMap.GetValueOrDefault(wireColor, Color.White);

            var idx = wireIndex;
            wireButton.OnPressed += _ => RaiseAction(new CutWireAction(idx));

            _wiresContainer.AddChild(wireButton);
        }
    }
}
