// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Client.UserInterface.Controls;
using Content.Trauma.Shared.Heretic.Components.Side;
using Content.Trauma.Shared.Heretic.Messages;
using JetBrains.Annotations;
// Dumont end

// Dumont start
using Robust.Client.UserInterface;
// Dumont end

namespace Content.Trauma.Client.Heretic.UI;

[UsedImplicitly]
public sealed partial class MawedCrucibleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private IPrototypeManager _proto = default!;

    private SimpleRadialMenu? _menu;

    protected override void Open()
    {
        base.Open();

        if (!EntMan.TryGetComponent(Owner, out MawedCrucibleComponent? crucible))
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        var buttonModels = ConvertToButtons(crucible.Potions);
        _menu.SetButtons(buttonModels);

        _menu.Open();
    }

    private IEnumerable<RadialMenuActionOption<EntProtoId>> ConvertToButtons(IReadOnlyList<EntProtoId> entProtoIds)
    {
        var models = new RadialMenuActionOption<EntProtoId>[entProtoIds.Count];
        for (var i = 0; i < entProtoIds.Count; i++)
        {
            var protoId = entProtoIds[i];
            var proto = _proto.Index(protoId);
            models[i] = new RadialMenuActionOption<EntProtoId>(HandleRadialMenuClick, protoId)
            {
                Sprite = new SpriteSpecifier.EntityPrototype(protoId),
                ToolTip = proto.Name,
            };
        }

        return models;
    }

    private void HandleRadialMenuClick(EntProtoId proto)
    {
        SendPredictedMessage(new MawedCrucibleMessage(proto));
    }
}
