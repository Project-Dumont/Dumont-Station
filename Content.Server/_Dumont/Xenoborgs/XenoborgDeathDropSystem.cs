// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Dumont.Xenoborgs;
using Content.Shared.Destructible;
using Content.Shared.Mobs;

namespace Content.Server._Dumont.Xenoborgs;

public sealed partial class XenoborgDeathDropSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoborgDeathDropComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<XenoborgDeathDropComponent, DestructionEventArgs>(OnDestroyed);
    }

    private void OnMobStateChanged(Entity<XenoborgDeathDropComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            TryDrop(ent);
    }

    private void OnDestroyed(EntityUid uid, XenoborgDeathDropComponent comp, DestructionEventArgs args)
    {
        TryDrop((uid, comp));
    }

    private void TryDrop(Entity<XenoborgDeathDropComponent> ent)
    {
        if (ent.Comp.Dropped)
            return;

        ent.Comp.Dropped = true;
        SpawnNextToOrDrop(ent.Comp.Proto, ent);
    }
}
