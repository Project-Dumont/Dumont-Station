// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Client.UserInterface.Controls;
using Content.Trauma.Shared.Heretic.Components.Side.Carvings;
using JetBrains.Annotations;
// Dumont end

// Dumont start
using Robust.Client.UserInterface;
using Robust.Client.GameObjects;
// Dumont end

namespace Content.Trauma.Client.Heretic.UI;

[UsedImplicitly]
public sealed partial class CarvingKnifeBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private IPrototypeManager _proto = default!;

    private SimpleRadialMenu? _menu;

    protected override void Open()
    {
        base.Open();

        if (!EntMan.TryGetComponent(Owner, out CarvingKnifeComponent? knife))
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        var buttonModels = ConvertToButtons(knife.Carvings);
        _menu.SetButtons(buttonModels);

        _menu.Open();
    }

    private IEnumerable<RadialMenuActionOption<EntProtoId>> ConvertToButtons(
        IReadOnlyList<EntProtoId> protos)
    {
        var models = new RadialMenuActionOption<EntProtoId>[protos.Count];
        for (var i = 0; i < protos.Count; i++)
        {
            var protoId = protos[i];
            var proto = _proto.Index(protoId);

            models[i] = new RadialMenuActionOption<EntProtoId>(HandleRadialMenuClick, protoId)
            {
                Sprite = proto.TryComp(out IconComponent? icon, EntMan.ComponentFactory)
                    ? icon.Icon
                    : new SpriteSpecifier.EntityPrototype(proto.ID),
                ToolTip = Loc.GetString("carving-knife-ui-tooltip", ("name", proto.Name), ("desc", proto.Description)),
            };
        }

        return models;
    }


    private void HandleRadialMenuClick(EntProtoId protoId)
    {
        SendMessage(new RuneCarvingSelectedMessage(protoId));
    }
}
