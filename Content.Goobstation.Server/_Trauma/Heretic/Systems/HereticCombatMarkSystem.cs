// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Components;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Cosmos;
using Content.Trauma.Shared.Heretic.Systems;
// Dumont end

namespace Content.Trauma.Server.Heretic.Systems;

public sealed class HereticCombatMarkSystem : SharedHereticCombatMarkSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCombatMarkComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HereticCombatMarkComponent, ComponentRemove>(OnRemove);

        SubscribeLocalEvent<HereticCosmicMarkComponent, ComponentRemove>(OnCosmicRemove);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = Timing.CurTime;

        var query = EntityQueryEnumerator<HereticCombatMarkComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now > comp.NextDisappear)
                RemComp(uid, comp);
        }
    }

    private void OnMapInit(Entity<HereticCombatMarkComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextDisappear = Timing.CurTime + ent.Comp.DisappearTime;
    }

    private void OnRemove(Entity<HereticCombatMarkComponent> ent, ref ComponentRemove args)
    {
        if (TerminatingOrDeleted(ent.Owner))
            return;

        RemComp<HereticCosmicMarkComponent>(ent.Owner);
    }

    private void OnCosmicRemove(Entity<HereticCosmicMarkComponent> ent, ref ComponentRemove args)
    {
        if (TerminatingOrDeleted(ent.Comp.CosmicDiamondUid))
            return;

        Del(ent.Comp.CosmicDiamondUid);
    }
}
