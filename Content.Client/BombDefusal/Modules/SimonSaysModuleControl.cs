using System.Linq;
using System.Numerics;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

/// <summary>
/// UI control for the "Simon Says" module.
/// Shows 4 colored buttons and displays the flash sequence.
/// Player must press the remapped colors in order.
/// </summary>
public sealed class SimonSaysModuleControl : BaseModuleControl
{
    private readonly Label _title;
    private readonly Label _stageLabel;
    private readonly Label _sequenceLabel;
    private readonly GridContainer _grid;
    private readonly Label _solvedLabel;

    private static readonly Dictionary<SimonColor, Color> SimonColorMap = new()
    {
        { SimonColor.Red, Color.FromHex("#ff3333") },
        { SimonColor.Blue, Color.FromHex("#3366ff") },
        { SimonColor.Green, Color.FromHex("#33ff33") },
        { SimonColor.Yellow, Color.FromHex("#ffff33") },
    };

    private static readonly Dictionary<SimonColor, string> SimonColorNames = new()
    {
        { SimonColor.Red, "RED" },
        { SimonColor.Blue, "BLUE" },
        { SimonColor.Green, "GREEN" },
        { SimonColor.Yellow, "YELLOW" },
    };

    public SimonSaysModuleControl()
    {
        _title = new Label
        {
            Text = Loc.GetString("bomb-defusal-module-simon"),
            FontColorOverride = Color.FromHex("#00ff41"),
            Margin = new Thickness(0, 0, 0, 4),
        };
        AddChild(_title);

        _stageLabel = new Label
        {
            Text = "Stage: 1/3",
            FontColorOverride = Color.FromHex("#aaaaaa"),
            Margin = new Thickness(0, 0, 0, 2),
        };
        AddChild(_stageLabel);

        _sequenceLabel = new Label
        {
            Text = "Sequence: ...",
            FontColorOverride = Color.FromHex("#ffaa00"),
            Margin = new Thickness(0, 0, 0, 4),
        };
        AddChild(_sequenceLabel);

        _grid = new GridContainer
        {
            Columns = 2,
            HorizontalExpand = true,
        };
        AddChild(_grid);

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
        if (state is not SimonSaysModuleState simonState)
            return;

        _solvedLabel.Visible = simonState.IsSolved;
        _stageLabel.Text = $"Stage: {simonState.CurrentStage + 1}/{simonState.TotalStages}";

        // Show the flash sequence as color names
        var sequenceStr = string.Join(" → ",
            simonState.FlashSequence.Select(c => SimonColorNames.GetValueOrDefault(c, "?")));
        _sequenceLabel.Text = $"Sequence: {sequenceStr}";

        // Rebuild the 4 color buttons
        _grid.RemoveAllChildren();

        foreach (var color in new[] { SimonColor.Red, SimonColor.Blue, SimonColor.Green, SimonColor.Yellow })
        {
            var btn = new Button
            {
                Text = SimonColorNames.GetValueOrDefault(color, "?"),
                MinSize = new Vector2(60, 40),
                Disabled = simonState.IsSolved,
                Margin = new Thickness(2),
                HorizontalExpand = true,
            };
            btn.ModulateSelfOverride = SimonColorMap.GetValueOrDefault(color, Color.Gray);

            var col = color;
            btn.OnPressed += _ => RaiseAction(new PressSimonColorAction(col));

            _grid.AddChild(btn);
        }
    }
}
