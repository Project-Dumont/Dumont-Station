using System.Linq;
using System.Numerics;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.Graphics;
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
    private readonly Label _stageLabel;
    private readonly BoxContainer _sequenceRow;
    private readonly GridContainer _grid;

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

    private readonly List<Button> _simonButtons = new();

    public SimonSaysModuleControl()
    {
        SetHeaderText(Loc.GetString("bomb-defusal-module-simon"));

        // Stage indicator
        _stageLabel = new Label
        {
            Text = "STAGE 1/3",
            FontColorOverride = Color.FromHex("#8888aa"),
            Margin = new Thickness(0, 0, 0, 4),
        };
        ContentContainer.AddChild(_stageLabel);

        // Flash sequence display as colored dots
        var sequencePanel = new PanelContainer
        {
            Margin = new Thickness(0, 0, 0, 6),
            HorizontalExpand = true,
        };
        sequencePanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#111122"),
            ContentMarginLeftOverride = 6,
            ContentMarginRightOverride = 6,
            ContentMarginTopOverride = 4,
            ContentMarginBottomOverride = 4,
        };

        var sequenceColumn = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };

        var sequenceTitle = new Label
        {
            Text = "SEQUENCE:",
            FontColorOverride = Color.FromHex("#666688"),
            Margin = new Thickness(0, 0, 0, 2),
        };
        sequenceColumn.AddChild(sequenceTitle);

        _sequenceRow = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
        };
        sequenceColumn.AddChild(_sequenceRow);

        sequencePanel.AddChild(sequenceColumn);
        ContentContainer.AddChild(sequencePanel);

        // 2x2 color button grid
        _grid = new GridContainer
        {
            Columns = 2,
            HorizontalExpand = true,
        };
        ContentContainer.AddChild(_grid);
    }

    public override void UpdateModuleState(BombDefusalModuleState state)
    {
        if (state is not SimonSaysModuleState simonState)
            return;

        _stageLabel.Text = $"STAGE {simonState.CurrentStage + 1}/{simonState.TotalStages}";

        // Build sequence display as colored blocks
        _sequenceRow.RemoveAllChildren();
        foreach (var flashColor in simonState.FlashSequence)
        {
            var dotPanel = new PanelContainer
            {
                MinWidth = 20,
                MinHeight = 16,
                Margin = new Thickness(1, 0),
            };
            dotPanel.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = SimonColorMap.GetValueOrDefault(flashColor, Color.Gray),
            };

            var dotLabel = new Label
            {
                Text = SimonColorNames.GetValueOrDefault(flashColor, "?")[..1],
                FontColorOverride = Color.FromHex("#000000"),
                Align = Label.AlignMode.Center,
            };
            dotPanel.AddChild(dotLabel);
            _sequenceRow.AddChild(dotPanel);
        }

        // Rebuild/Update the 4 color buttons
        if (_simonButtons.Count == 0)
        {
            _grid.RemoveAllChildren();

            foreach (var color in new[] { SimonColor.Red, SimonColor.Blue, SimonColor.Green, SimonColor.Yellow })
            {
                var buttonPanel = new PanelContainer
                {
                    Margin = new Thickness(2),
                    HorizontalExpand = true,
                };
                buttonPanel.PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = Color.FromHex("#111122"),
                    BorderColor = Color.FromHex("#333355"),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 2,
                    ContentMarginRightOverride = 2,
                    ContentMarginTopOverride = 2,
                    ContentMarginBottomOverride = 2,
                };

                var btn = new Button
                {
                    Text = SimonColorNames.GetValueOrDefault(color, "?"),
                    MinSize = new Vector2(70, 45),
                    HorizontalExpand = true,
                };
                btn.ModulateSelfOverride = SimonColorMap.GetValueOrDefault(color, Color.Gray);

                var col = color;
                btn.OnPressed += _ => RaiseAction(new PressSimonColorAction(col));

                buttonPanel.AddChild(btn);
                _grid.AddChild(buttonPanel);
                _simonButtons.Add(btn);
            }
        }

        foreach (var btn in _simonButtons)
        {
            btn.Disabled = simonState.IsSolved;
        }
    }
}
