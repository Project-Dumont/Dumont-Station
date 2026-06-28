using System.Collections.Generic;
using System.Numerics;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

public sealed class PasswordModuleControl : BaseModuleControl
{
    private readonly List<Label> _letterLabels = new();
    private readonly List<Button> _upButtons = new();
    private readonly List<Button> _downButtons = new();
    private readonly Button _btnSubmit;

    public PasswordModuleControl()
    {
        SetHeaderText(Loc.GetString("bomb-defusal-module-password"));

        var mainLayout = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Align = BoxContainer.AlignMode.Center,
        };
        ContentContainer.AddChild(mainLayout);

        // Columns layout
        var columnsLayout = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8),
        };
        mainLayout.AddChild(columnsLayout);

        for (int i = 0; i < 5; i++)
        {
            var colIdx = i;
            var colLayout = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Margin = new Thickness(2, 0),
            };
            columnsLayout.AddChild(colLayout);

            var btnUp = new Button
            {
                Text = "▲",
                MinSize = new Vector2(26, 20),
            };
            btnUp.OnPressed += _ => RaiseAction(new CyclePasswordColumnAction(colIdx, true));
            colLayout.AddChild(btnUp);
            _upButtons.Add(btnUp);

            var charPanel = new PanelContainer
            {
                MinSize = new Vector2(26, 32),
                Margin = new Thickness(0, 4),
            };
            charPanel.PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#000000"),
                BorderColor = Color.FromHex("#333355"),
                BorderThickness = new Thickness(1),
            };

            var label = new Label
            {
                Text = "A",
                HorizontalAlignment = HAlignment.Center,
                VerticalAlignment = VAlignment.Center,
                FontColorOverride = Color.FromHex("#00ff41"),
            };
            charPanel.AddChild(label);
            colLayout.AddChild(charPanel);
            _letterLabels.Add(label);

            var btnDown = new Button
            {
                Text = "▼",
                MinSize = new Vector2(26, 20),
            };
            btnDown.OnPressed += _ => RaiseAction(new CyclePasswordColumnAction(colIdx, false));
            colLayout.AddChild(btnDown);
            _downButtons.Add(btnDown);
        }

        // Submit button
        _btnSubmit = new Button
        {
            Text = "SUBMIT",
            MinSize = new Vector2(80, 28),
            HorizontalAlignment = HAlignment.Center,
        };
        _btnSubmit.OnPressed += _ => RaiseAction(new SubmitPasswordAction());
        mainLayout.AddChild(_btnSubmit);
    }

    public override void UpdateModuleState(BombDefusalModuleState state)
    {
        if (state is not PasswordModuleState passState)
            return;

        var solved = passState.IsSolved;

        for (int i = 0; i < 5; i++)
        {
            _upButtons[i].Disabled = solved;
            _downButtons[i].Disabled = solved;

            if (i < passState.Columns.Count && i < passState.SelectedIndices.Count)
            {
                var letterList = passState.Columns[i];
                var selIdx = passState.SelectedIndices[i];
                if (selIdx >= 0 && selIdx < letterList.Count)
                {
                    _letterLabels[i].Text = letterList[selIdx].ToString();
                }
                else
                {
                    _letterLabels[i].Text = "?";
                }
            }
            else
            {
                _letterLabels[i].Text = "?";
            }
        }

        _btnSubmit.Disabled = solved;
    }
}
