using System.Collections.Generic;
using System.Numerics;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

public sealed class WhosOnFirstModuleControl : BaseModuleControl
{
    private readonly Label _stageLabel;
    private readonly Label _displayScreen;
    private readonly List<Button> _buttons = new();

    public WhosOnFirstModuleControl()
    {
        SetHeaderText(Loc.GetString("bomb-defusal-module-whosonfirst"));

        var mainLayout = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Align = BoxContainer.AlignMode.Center,
        };
        ContentContainer.AddChild(mainLayout);

        // Stage label
        _stageLabel = new Label
        {
            Text = "STAGE 1 OF 3",
            FontColorOverride = Color.FromHex("#8888aa"),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 4),
        };
        mainLayout.AddChild(_stageLabel);

        // Screen
        var screenPanel = new PanelContainer
        {
            MinSize = new Vector2(130, 40),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8),
        };
        screenPanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#000000"),
            BorderColor = Color.FromHex("#333355"),
            BorderThickness = new Thickness(1),
        };

        _displayScreen = new Label
        {
            Text = "READY",
            FontColorOverride = Color.FromHex("#00ff41"),
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
        };
        screenPanel.AddChild(_displayScreen);
        mainLayout.AddChild(screenPanel);

        // Buttons 3x2 grid
        var grid = new GridContainer
        {
            Columns = 2,
            HorizontalAlignment = HAlignment.Center,
        };
        mainLayout.AddChild(grid);

        for (int i = 0; i < 6; i++)
        {
            var btnIndex = i;
            var button = new Button
            {
                Text = "?",
                MinSize = new Vector2(80, 30),
                Margin = new Thickness(2, 2),
            };
            button.OnPressed += _ => RaiseAction(new PressWhosOnFirstButtonAction(btnIndex));
            grid.AddChild(button);
            _buttons.Add(button);
        }
    }

    public override void UpdateModuleState(BombDefusalModuleState state)
    {
        if (state is not WhosOnFirstModuleState wofState)
            return;

        var stage = wofState.CurrentStage;
        if (stage >= 3)
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

        _stageLabel.Text = $"STAGE {stage + 1} OF 3";
        _displayScreen.Text = wofState.DisplayWord;

        var solved = wofState.IsSolved;

        for (int i = 0; i < 6; i++)
        {
            _buttons[i].Disabled = solved;
            if (i < wofState.ButtonLabels.Count)
            {
                _buttons[i].Text = wofState.ButtonLabels[i];
            }
            else
            {
                _buttons[i].Text = "?";
            }
        }
    }
}
