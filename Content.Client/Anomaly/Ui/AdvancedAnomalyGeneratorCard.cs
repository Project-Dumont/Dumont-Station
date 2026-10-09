// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Message;
using Content.Shared.Anomaly.Prototypes;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Prototypes;

namespace Content.Client.Anomaly.Ui;

public sealed class AdvancedAnomalyGeneratorCard : PanelContainer
{
    public readonly ProtoId<AdvancedAnomalyGenerationPrototype> Entry;

    public event Action? OnSelected;

    public bool Selected
    {
        get => PanelOverride == AdvancedAnomalyGeneratorTheme.SelectedCard;
        set => PanelOverride = value ? AdvancedAnomalyGeneratorTheme.SelectedCard : AdvancedAnomalyGeneratorTheme.Card;
    }

    public AdvancedAnomalyGeneratorCard(AdvancedAnomalyGenerationPrototype entry, string materialName)
    {
        Entry = entry.ID;
        HorizontalExpand = true;
        MouseFilter = MouseFilterMode.Stop;
        PanelOverride = AdvancedAnomalyGeneratorTheme.Card;

        var research = new RichTextLabel();
        research.SetMarkup(Loc.GetString("advanced-anomaly-generator-ui-card-research", ("cost", entry.ResearchCost)));

        var material = new RichTextLabel();
        material.SetMarkup(Loc.GetString("advanced-anomaly-generator-ui-card-material",
            ("cost", entry.MaterialCost), ("material", materialName)));

        var preview = new EntityPrototypeView
        {
            SetSize = new Vector2(64, 64),
            OverrideDirection = Direction.South,
            VerticalAlignment = VAlignment.Center,
        };
        preview.SetPrototype(entry.AnomalyPrototype);

        AddChild(new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 10,
            Margin = new Thickness(6, 5),
            Children =
            {
                preview,
                new BoxContainer
                {
                    Orientation = BoxContainer.LayoutOrientation.Vertical,
                    VerticalAlignment = VAlignment.Center,
                    HorizontalExpand = true,
                    SeparationOverride = 4,
                    Children = { new Label { Text = Loc.GetString(entry.Name) }, research, material },
                },
            },
        });

        // The whole card is clickable, so the stuff inside just lets the click pass through
        OnKeyBindDown += args =>
        {
            if (args.Function == EngineKeyFunctions.UIClick)
                OnSelected?.Invoke();
        };
    }
}
