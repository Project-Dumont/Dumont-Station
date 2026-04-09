// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Zombies;
using Content.Shared._Gabystation.SmartZombie;
using Content.Shared.Weapons.Melee;
using Content.Shared.Zombies;

namespace Content.Server._Gabystation.SmartZombie;

public sealed class SmartZombieSystem : SmartZombieSystemShared {
    [Dependency] private readonly ZombieSystem _zombie = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SmartZombieComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SmartZombieComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, SmartZombieComponent smartass, ComponentStartup args)
    {
        if (TryComp<ZombieComponent>(uid, out var comp))
            _zombie.UnZombify(uid, uid, comp);

        AddComp<NonSpreaderZombieComponent>(uid);   // ts chud is whitewashed
        _zombie.ZombifyEntity(uid);
        var zed = Comp<ZombieComponent>(uid);

        zed.PassiveHealing *= smartass.HealModifier;
        Dirty(uid, zed);

        var melee = Comp<MeleeWeaponComponent>(uid);
        melee.Damage *= smartass.DamageModifier;
        Dirty(uid, melee);
    }

    private void OnShutdown(EntityUid uid, SmartZombieComponent smartass, ComponentShutdown args)
    {
        // lowkey purging xer shi
        RemComp<NonSpreaderZombieComponent>(uid);
        if (TryComp<ZombieComponent>(uid, out var comp))
            // GARBAGE API FAHHHHHHHHHHHH
            _zombie.UnZombify(uid, uid, comp);
    }
}
