// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Message;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Maths;

namespace Content.Client.Anomaly.Ui;

public sealed class AnomalyGeneratorEntryControl : BoxContainer
{
    private static readonly Color SelectedTint = Color.FromHex("#44cc44").WithAlpha(0.22f);

    public readonly SpriteView SpritePreview;

    private readonly PanelContainer _background;
    private bool _selected;

    public event Action? OnSelected;

    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            _background.ModulateSelfOverride = value ? SelectedTint : null;
        }
    }

    public AnomalyGeneratorEntryControl(string name, string researchLine, string plasmaLine)
    {
        HorizontalExpand = true;
        MouseFilter = MouseFilterMode.Pass;

        _background = new PanelContainer { HorizontalExpand = true };
        _background.StyleClasses.Add("Inset");
        _background.MouseFilter = MouseFilterMode.Ignore;

        var row = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            SeparationOverride = 10,
            Margin = new Thickness(6, 5),
            HorizontalExpand = true,
            MouseFilter = MouseFilterMode.Ignore
        };

        SpritePreview = new SpriteView
        {
            SetSize = new Vector2(64, 64),
            OverrideDirection = Direction.South,
            VerticalAlignment = VAlignment.Center,
            MouseFilter = MouseFilterMode.Ignore
        };

        var info = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            VerticalAlignment = VAlignment.Center,
            HorizontalExpand = true,
            SeparationOverride = 4,
            MouseFilter = MouseFilterMode.Ignore
        };

        var nameLabel = new Label
        {
            Text = name,
            FontColorOverride = Color.White,
            MouseFilter = MouseFilterMode.Ignore
        };

        var researchLabel = new RichTextLabel { MouseFilter = MouseFilterMode.Ignore };
        researchLabel.SetMarkup(researchLine);

        var plasmaLabel = new RichTextLabel { MouseFilter = MouseFilterMode.Ignore };
        plasmaLabel.SetMarkup(plasmaLine);

        info.AddChild(nameLabel);
        info.AddChild(researchLabel);
        info.AddChild(plasmaLabel);

        row.AddChild(SpritePreview);
        row.AddChild(info);
        _background.AddChild(row);
        AddChild(_background);

        OnKeyBindDown += OnCardKeyBindDown;
    }

    private void OnCardKeyBindDown(GUIBoundKeyEventArgs args)
    {
        if (args.Function == EngineKeyFunctions.UIClick)
            OnSelected?.Invoke();
    }
}
