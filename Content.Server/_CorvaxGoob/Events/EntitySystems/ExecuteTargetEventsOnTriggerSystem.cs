// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 KillanGenifer <157119956+KillanGenifer@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._CorvaxGoob.Events.Components;
using Content.Shared.Trigger;

namespace Content.Server._CorvaxGoob.Events.EntitySystems;

public sealed class ExecuteTargetEventsOnTriggerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteTargetEventsOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(Entity<ExecuteTargetEventsOnTriggerComponent> entity, ref TriggerEvent ev)
    {
        foreach (var targetEvent in entity.Comp.Events)
        {
            targetEvent.Target = entity;
            RaiseLocalEvent(entity, (object) targetEvent, true);
        }
    }
}
