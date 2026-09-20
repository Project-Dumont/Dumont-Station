// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;

namespace Content.Shared.Projectiles;

public sealed class ComplexProjectileDamageSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ComplexProjectileDamageComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    private void OnProjectileHit(Entity<ComplexProjectileDamageComponent> ent, ref ProjectileHitEvent args)
    {
        foreach (var option in ent.Comp.DamageOptions)
        {
            if (!_whitelist.CheckBoth(args.Target, option.Blacklist, option.Whitelist))
                continue;

            args.Damage = option.Damage;
            return;
        }
    }
}
