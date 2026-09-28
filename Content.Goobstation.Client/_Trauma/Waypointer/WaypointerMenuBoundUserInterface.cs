// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Client.UserInterface.Controls;
using Content.Shared.Actions.Components;
using Content.Trauma.Shared.Waypointer;
using Content.Trauma.Shared.Waypointer.Components;
using Content.Trauma.Shared.Waypointer.Events;
using JetBrains.Annotations;
// Dumont end

// Dumont start
using Robust.Client.GameObjects;
// Dumont end

// Dumont start
using Robust.Client.UserInterface;
// Dumont end

namespace Content.Trauma.Client.Waypointer;

[UsedImplicitly]
public sealed partial class WaypointerMenuBoundUserinterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private IPrototypeManager _proto = default!;

    private SimpleRadialMenu? _menu;

    protected override void Open()
    {
        base.Open();

        // The owner is the action entity - Not the entity that has the waypointer.
        if (!EntMan.TryGetComponent<ActionComponent>(Owner, out var actionComp)
            || !EntMan.TryGetComponent<ActiveWaypointerComponent>(actionComp.Container, out var waypointer))
            return;

        var waypointers = CreateButtons(waypointer);

        if (waypointers == null)
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.SetButtons(waypointers);

        _menu.OpenCentered();
    }

    private IEnumerable<RadialMenuOption>? CreateButtons(ActiveWaypointerComponent waypointer)
    {
        if (waypointer.WaypointerProtoIds == null)
            return null;

        var options = new List<RadialMenuOption>();
        // We cannot use sprite specifier as we aren't using entities nor do we only need one image.
        // We need one for disabling and one for enabling - So we have this Frankenstein Monster.
        var state = waypointer.Active ? "action_icon_off"  : "action_icon_on";
        var sprite = new SpriteSpecifier.Rsi(waypointer.RadialMenuIconPath, state);
        var toggleWaypointers = new RadialMenuActionOption<bool>(HandleRadialMenuClick, !waypointer.Active)
        {
            Sprite = sprite,
            ToolTip = Loc.GetString(waypointer.Active ? "waypointer-disable-all" : "waypointer-enable-all"),
        };
        options.Add(toggleWaypointers);
        // This iterates through every waypointer to add them as options.
        foreach (var pair in waypointer.WaypointerProtoIds)
        {
            if (!_proto.Resolve(pair.Key, out var prototype))
                continue;
            // If the waypointer is active, we want to the get sprite for disabling it.
            var waypointerState = pair.Value ? "disable"  : "enable";
            var waypointerSprite = new SpriteSpecifier.Rsi(prototype.RadialMenuIconPath, waypointerState);
            var toggleWaypointer = new RadialMenuActionOption<ProtoId<WaypointerPrototype>>(HandleRadialMenuClick, pair.Key)
            {
                Sprite = waypointerSprite,
                ToolTip = Loc.GetString(pair.Value ? "waypointer-disable" : "waypointer-enable", ("waypointer", prototype.Name)),
            };
            options.Add(toggleWaypointer);
        }

        return options;
    }

    private void HandleRadialMenuClick(bool toggleAll)
    {
        var message = new WaypointersToggledMessage(toggleAll);
        SendPredictedMessage(message);
    }

    private void HandleRadialMenuClick(ProtoId<WaypointerPrototype> waypointer)
    {
        var message = new WaypointerStatusChangedMessage(waypointer);
        SendPredictedMessage(message);
    }
}
