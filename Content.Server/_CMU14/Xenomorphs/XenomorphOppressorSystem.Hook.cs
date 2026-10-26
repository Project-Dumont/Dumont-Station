using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphOppressorHookProjectileComponent : Component
{
    public MapCoordinates Origin;
}

public sealed partial class XenomorphOppressorSystem
{
    [Dependency] private SharedGunSystem _guns = default!;

    private void InitializeHook()
    {
        SubscribeLocalEvent<XenomorphOppressorComponent, XenomorphTailSeizeActionEvent>(OnSeize);
        SubscribeLocalEvent<XenomorphOppressorHookProjectileComponent, ProjectileHitEvent>(OnHookHit);
        SubscribeLocalEvent<XenomorphOppressorHookProjectileComponent, PreventCollideEvent>(OnHookCollide);
    }

    private void OnSeize(Entity<XenomorphOppressorComponent> ent, ref XenomorphTailSeizeActionEvent args)
    {
        if (args.Handled || !Ready(ent)) return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var direction = target.Position - origin.Position;
        if (origin.MapId != target.MapId || !float.IsFinite(direction.LengthSquared()) || direction.LengthSquared() < 0.0001f) return;
        if (TryComp<MeleeWeaponComponent>(ent, out var melee))
        {
            if (_timing.CurTime < melee.NextAttack) return;
            melee.NextAttack = _timing.CurTime + TimeSpan.FromSeconds(1);
            Dirty(ent, melee);
        }
        var projectile = Spawn("DumontXenomorphOppressorHook", _transform.GetMoverCoordinates(ent));
        Comp<XenomorphOppressorHookProjectileComponent>(projectile).Origin = origin;
        var tether = EnsureComp<XenomorphTailTetherComponent>(projectile);
        tether.Source = ent;
        Dirty(projectile, tether);
        _guns.ShootProjectile(projectile, direction, Vector2.Zero, ent, ent, 30);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/oppressor_tail.ogg"), ent);
        var attack = new MeleeAttackEvent(ent);
        RaiseLocalEvent(ent, ref attack);
        args.Handled = true;
    }

    private void OnHookCollide(Entity<XenomorphOppressorHookProjectileComponent> ent, ref PreventCollideEvent args)
    {
        if (HasComp<XenomorphComponent>(args.OtherEntity) || _mobs.IsDead(args.OtherEntity)) args.Cancelled = true;
    }

    private void OnHookHit(Entity<XenomorphOppressorHookProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (Vector2.DistanceSquared(_transform.GetMapCoordinates(ent).Position, ent.Comp.Origin.Position) > 3.5f * 3.5f)
        {
            args.Damage = new Content.Shared.Damage.DamageSpecifier();
            return;
        }
        if (args.Shooter is not { } shooter || !HasComp<XenomorphOppressorComponent>(shooter) || !Victim(args.Target)) return;
        Hook(shooter, args.Target, true);
    }

    public override void Update(float frameTime)
    {
        var projectiles = EntityQueryEnumerator<XenomorphOppressorHookProjectileComponent>();
        while (projectiles.MoveNext(out var uid, out var projectile))
        {
            var position = _transform.GetMapCoordinates(uid);
            if (position.MapId != projectile.Origin.MapId || Vector2.DistanceSquared(position.Position, projectile.Origin.Position) > 3.5f * 3.5f) QueueDel(uid);
        }
        var tethers = EntityQueryEnumerator<XenomorphTailTetherComponent>();
        while (tethers.MoveNext(out var uid, out var tether))
        {
            if (!TerminatingOrDeleted(tether.Source) && Transform(uid).MapID == Transform(tether.Source).MapID) continue;
            if (HasComp<XenomorphOppressorHookProjectileComponent>(uid)) QueueDel(uid);
            else RemCompDeferred<XenomorphTailTetherComponent>(uid);
        }
    }
}
