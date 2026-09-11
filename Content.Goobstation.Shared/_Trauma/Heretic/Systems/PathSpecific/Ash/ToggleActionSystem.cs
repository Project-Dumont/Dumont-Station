// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Actions;
using Content.Shared.Toggleable;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Ash;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Systems.PathSpecific.Ash;

public sealed partial class ToggleActionSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToggleActionComponent, ToggleActionEvent>(OnToggle);
    }

    private void OnToggle(Entity<ToggleActionComponent> ent, ref ToggleActionEvent args)
    {
        _actions.SetToggled(args.Action.AsNullable(), !args.Action.Comp.Toggled);
    }
}
