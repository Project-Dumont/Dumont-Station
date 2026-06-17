using System.Collections.Generic;
using System.Numerics;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

public sealed class MemoryModuleControl : BaseModuleControl
{
    private readonly Label _stageLabel;
    private readonly Label _displayScreen;
    private readonly List<Button> _buttons = new();

    public MemoryModuleControl()
    {
        SetHeaderText(Loc.GetString("bomb-defusal-module-memory"));

        var mainLayout = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Align = BoxContainer.AlignMode.Center,
        };
        ContentContainer.AddChild(mainLayout);

        // Header/Stage progress label
        _stageLabel = new Label
        {
            Text = "STAGE 1 OF 5",
            FontColorOverride = Color.FromHex("#8888aa"),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8),
        };
        mainLayout.AddChild(_stageLabel);

        // Digital screen
        var screenPanel = new PanelContainer
        {
            MinSize = new Vector2(120, 60),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16),
        };
        screenPanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#000000"),
            BorderColor = Color.FromHex("#333355"),
            BorderThickness = new Thickness(1),
        };

        _displayScreen = new Label
        {
            Text = "1",
            FontColorOverride = Color.FromHex("#00ff41"),
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
        };
        screenPanel.AddChild(_displayScreen);
        mainLayout.AddChild(screenPanel);

        // Buttons row
        var buttonsRow = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalAlignment = HAlignment.Center,
        };
        mainLayout.AddChild(buttonsRow);

        for (int i = 0; i < 4; i++)
        {
            var btnIndex = i;
            var button = new Button
            {
                Text = "?",
                MinSize = new Vector2(45, 45),
                Margin = new Thickness(4, 0),
            };
            button.OnPressed += _ => RaiseAction(new PressMemoryButtonAction(btnIndex));
            buttonsRow.AddChild(button);
            _buttons.Add(button);
        }
    }

    public override void UpdateModuleState(BombDefusalModuleState state)
    {
        if (state is not MemoryModuleState memoryState)
            return;

        var stage = memoryState.CurrentStage;
        if (stage >= 5)
        {
            _stageLabel.Text = "SOLVED";
            _displayScreen.Text = "---";
            foreach (var btn in _buttons)
            {
                btn.Disabled = true;
                btn.Text = "";
            }
            return;
        }

        _stageLabel.Text = $"STAGE {stage + 1} OF 5";
        _displayScreen.Text = memoryState.DisplayNumber.ToString();

        var solved = memoryState.IsSolved;

        for (int i = 0; i < 4; i++)
        {
            _buttons[i].Disabled = solved;
            if (i < memoryState.ButtonLabels.Count)
            {
                _buttons[i].Text = memoryState.ButtonLabels[i].ToString();
            }
            else
            {
                _buttons[i].Text = "?";
            }
        }
    }
}
