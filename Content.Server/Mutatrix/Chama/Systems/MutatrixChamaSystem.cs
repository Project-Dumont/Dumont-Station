// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Server.Mutatrix.Chama.Components;
using Content.Shared.Actions;
using Content.Shared.Atmos.Components;
using Content.Shared.Mutatrix.Chama.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;

namespace Content.Server.Mutatrix.Chama.Systems;

public sealed class MutatrixChamaSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MutatrixChamaComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MutatrixChamaComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MutatrixChamaComponent, MutatrixChamaFireballActionEvent>(OnFireball);
        SubscribeLocalEvent<MutatrixChamaComponent, MutatrixChamaFlameActionEvent>(OnFlame);
    }

    private void OnMapInit(Entity<MutatrixChamaComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.FireballActionEntity, ent.Comp.FireballAction);
        _actions.AddAction(ent, ref ent.Comp.FlameActionEntity, ent.Comp.FlameAction);

        ent.Comp.FireballGun = Spawn(ent.Comp.FireballGunProto);
        ent.Comp.FlameGun = Spawn(ent.Comp.FlameGunProto);

        IgniteChama(ent);
    }

    private void OnShutdown(Entity<MutatrixChamaComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.FireballGun is { } fireballGun)
            QueueDel(fireballGun);

        if (ent.Comp.FlameGun is { } flameGun)
            QueueDel(flameGun);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MutatrixChamaComponent>();
        while (query.MoveNext(out var uid, out var chama))
        {
            chama.FireCheckAccumulator += frameTime;
            if (chama.FireCheckAccumulator < 1f)
                continue;

            chama.FireCheckAccumulator = 0f;
            IgniteChama((uid, chama));
        }
    }

    private void IgniteChama(Entity<MutatrixChamaComponent> ent)
    {
        if (!TryComp<FlammableComponent>(ent, out var flammable))
            return;

        if (flammable.FireStacks < ent.Comp.FireStacks)
            _flammable.SetFireStacks(ent, ent.Comp.FireStacks, flammable, ignite: true);

        if (!flammable.OnFire)
            _flammable.Ignite(ent, ent, flammable);
    }

    private void OnFireball(Entity<MutatrixChamaComponent> ent, ref MutatrixChamaFireballActionEvent args)
    {
        Shoot(ent, ent.Comp.FireballGun, args.Target);
    }

    private void OnFlame(Entity<MutatrixChamaComponent> ent, ref MutatrixChamaFlameActionEvent args)
    {
        Shoot(ent, ent.Comp.FlameGun, args.Target);
    }

    private void Shoot(Entity<MutatrixChamaComponent> ent, EntityUid? gunUid, EntityCoordinates target)
    {
        if (gunUid == null)
            return;

        if (!TryComp<GunComponent>(gunUid.Value, out var gun))
            return;

        _gun.AttemptShoot(ent, gunUid.Value, gun, target);
    }
}
