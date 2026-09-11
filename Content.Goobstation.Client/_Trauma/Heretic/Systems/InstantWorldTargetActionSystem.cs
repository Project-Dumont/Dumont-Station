// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Client.Actions;
using Content.Shared.Actions.Components;
using Content.Trauma.Common.Heretic;
using Content.Trauma.Shared.Heretic.Events;
using Content.Trauma.Shared.Heretic.Systems;
// Dumont end

namespace Content.Trauma.Client.Heretic.Systems;

public sealed partial class InstantWorldTargetActionSystem : SharedInstantWorldTargetActionSystem
{
    [Dependency] private ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionComponent, TryPerformInstantWorldTargetActionEvent>(OnTryPerform);
    }

    private void OnTryPerform(Entity<ActionComponent> ent, ref TryPerformInstantWorldTargetActionEvent args)
    {
        if (TryComp(ent, out WorldTargetActionComponent? comp) && comp.Event is InstantWorldTargetActionEvent)
            _actions.TriggerAction(ent, true);
    }
}
