// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Dumont.DeathCutscene;
using Content.Shared._ES.DeathCutscene;
using Content.Shared.Implants.Components;

namespace Content.Server._Dumont.DeathCutscene;

public sealed class BlocksDeathCutsceneSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ImplantedComponent, DeathCutsceneAttemptEvent>(OnDeathCutsceneAttempt);
    }

    private void OnDeathCutsceneAttempt(Entity<ImplantedComponent> ent, ref DeathCutsceneAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        foreach (var implant in ent.Comp.ImplantContainer.ContainedEntities)
        {
            if (!HasComp<BlocksDeathCutsceneComponent>(implant))
                continue;

            args.Cancelled = true;
            return;
        }
    }
}
