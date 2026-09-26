// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Emp;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server.Weapons.Ranged.Systems;

/// <summary>
/// Refills the ammo of a gun with <see cref="BallisticAmmoSelfRefillerComponent"/> over time.
/// </summary>
public sealed class BallisticAmmoSelfRefillerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BallisticAmmoSelfRefillerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BallisticAmmoSelfRefillerComponent, EmpPulseEvent>(OnEmpPulse);
    }

    private void OnMapInit(Entity<BallisticAmmoSelfRefillerComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextAutoRefill = _timing.CurTime + ent.Comp.AutoRefillRate;
    }

    private void OnEmpPulse(Entity<BallisticAmmoSelfRefillerComponent> ent, ref EmpPulseEvent args)
    {
        if (!ent.Comp.AffectedByEmp)
            return;

        args.Affected = true;
        ent.Comp.NextAutoRefill = _timing.CurTime + args.Duration;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BallisticAmmoSelfRefillerComponent, BallisticAmmoProviderComponent>();
        while (query.MoveNext(out var uid, out var refiller, out var ammo))
        {
            if (_timing.CurTime < refiller.NextAutoRefill)
                continue;

            refiller.NextAutoRefill += refiller.AutoRefillRate;

            if (!refiller.AutoRefill || ammo.Count >= ammo.Capacity)
                continue;

            if ((refiller.AmmoProto ?? ammo.Proto) == null)
                continue;

            _gun.SetBallisticUnspawned((uid, ammo), ammo.UnspawnedCount + 1);
            _audio.PlayPvs(refiller.RechargeSound, uid);
        }
    }
}
