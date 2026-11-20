// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using System.Linq;
using Content.Client.UserInterface.Controls;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Lock;
using Content.Trauma.Shared.Heretic.Rituals;
using JetBrains.Annotations;
// Dumont end

// Dumont start
using Robust.Client.UserInterface;
// Dumont end

namespace Content.Trauma.Client.Heretic.UI;

[UsedImplicitly]
public sealed partial class EldritchIdBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private IPrototypeManager _proto = default!;

    private SimpleRadialMenu? _menu;

    protected override void Open()
    {
        base.Open();

        if (!EntMan.TryGetComponent(Owner, out EldritchIdCardComponent? id))
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        var buttonModels = ConvertToButtons(id.Configs.ToList());
        _menu.SetButtons(buttonModels);

        _menu.Open();
    }

    private IEnumerable<RadialMenuActionOption<EldritchIdConfiguration>> ConvertToButtons(
        IReadOnlyList<EldritchIdConfiguration> configs)
    {
        var models = new RadialMenuActionOption<EldritchIdConfiguration>[configs.Count];
        for (var i = 0; i < configs.Count; i++)
        {
            var config = configs[i];
            var proto = _proto.Index(config.CardPrototype);

            var jobSuffix = string.IsNullOrWhiteSpace(config.JobTitle) ? string.Empty : $" ({config.JobTitle})";

            var val = string.IsNullOrWhiteSpace(config.FullName)
                ? Loc.GetString("access-id-card-component-owner-name-job-title-text",
                    ("jobSuffix", jobSuffix))
                : Loc.GetString("access-id-card-component-owner-full-name-job-title-text",
                    ("fullName", config.FullName),
                    ("jobSuffix", jobSuffix));

            models[i] = new RadialMenuActionOption<EldritchIdConfiguration>(HandleRadialMenuClick, config)
            {
                Sprite = new SpriteSpecifier.EntityPrototype(proto.ID),
                ToolTip = val,
            };
        }

        return models;
    }

    private void HandleRadialMenuClick(EldritchIdConfiguration config)
    {
        SendPredictedMessage(new EldritchIdMessage(config));
    }
}
