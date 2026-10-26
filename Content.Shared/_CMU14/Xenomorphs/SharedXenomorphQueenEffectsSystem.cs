using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Damage;
using Content.Shared.Hands.Components;
using Content.Shared.Hands;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphQueenEffectsSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private MobThresholdSystem _thresholds = default!;
    [Dependency] private SharedGunSystem _guns = default!;
    [Dependency] private SharedContainerSystem _containers = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphPheromoneEffectComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphPheromoneEffectComponent, GetMeleeDamageEvent>(OnDamage);
        SubscribeLocalEvent<XenomorphPheromoneEffectComponent, XenomorphTailDamageEvent>(OnTail);
        SubscribeLocalEvent<XenomorphPheromoneEffectComponent, UpdateMobStateEvent>(OnMobState, after: new[] { typeof(MobThresholdSystem) });
        SubscribeLocalEvent<XenomorphPheromoneEffectComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphPheromoneEffectComponent, ComponentShutdown>(OnRemoved);
        SubscribeLocalEvent<XenomorphQueenScreechBlindComponent, ComponentStartup>(OnBlind);
        SubscribeLocalEvent<XenomorphQueenScreechBlindComponent, ComponentShutdown>(OnUnblind);
        SubscribeLocalEvent<XenomorphQueenScreechBlindComponent, DidEquipHandEvent>(OnEquip);
        SubscribeLocalEvent<XenomorphQueenScreechBlindComponent, DidUnequipHandEvent>(OnUnequip);
        SubscribeLocalEvent<XenomorphQueenScreechScatterComponent, GunRefreshModifiersEvent>(OnScatter);
    }
    private void OnSpeed(Entity<XenomorphPheromoneEffectComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var speed = 1 + ent.Comp.Frenzy * (TryComp<PullerComponent>(ent, out var puller) && puller.Pulling != null ? .03f : .06f);
        args.ModifySpeed(speed, speed);
    }
    private void OnDamage(Entity<XenomorphPheromoneEffectComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (ent.Comp.Frenzy <= 0) return;
        var bonus = ent.Comp.Frenzy * 2 / 3;
        args.Damage += new DamageSpecifier { DamageDict = { ["Blunt"] = bonus, ["Slash"] = bonus, ["Piercing"] = bonus } };
    }
    private void OnTail(Entity<XenomorphPheromoneEffectComponent> ent, ref XenomorphTailDamageEvent args)
    {
        if (ent.Comp.Frenzy <= 0) return;
        var bonus = ent.Comp.Frenzy * 2 * 1.2f / 3;
        args.Damage += new DamageSpecifier { DamageDict = { ["Blunt"] = bonus, ["Slash"] = bonus, ["Piercing"] = bonus } };
    }
    private void OnMobState(Entity<XenomorphPheromoneEffectComponent> ent, ref UpdateMobStateEvent args)
    {
        if (ent.Comp.Warding <= 0 || args.Component.CurrentState == MobState.Dead || args.State != MobState.Dead
            || !TryComp<DamageableComponent>(ent, out var damage)
            || !_thresholds.TryGetThresholdForState(ent, MobState.Dead, out var dead)) return;
        if (damage.TotalDamage < dead!.Value + 1 + 20 * ent.Comp.Warding) args.State = MobState.Critical;
    }
    private void OnState(Entity<XenomorphPheromoneEffectComponent> ent, ref AfterAutoHandleStateEvent args) => _movement.RefreshMovementSpeedModifiers(ent);
    private void OnRemoved(Entity<XenomorphPheromoneEffectComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.Frenzy = 0;ent.Comp.Warding = 0;ent.Comp.Recovery = 0;
        _movement.RefreshMovementSpeedModifiers(ent);
        if (TryComp<Content.Shared.Mobs.Components.MobThresholdsComponent>(ent, out var thresholds)) _thresholds.VerifyThresholds(ent, thresholds);
    }
    private void OnBlind(Entity<XenomorphQueenScreechBlindComponent> ent, ref ComponentStartup args) => AddScatter(ent);
    private void OnEquip(Entity<XenomorphQueenScreechBlindComponent> ent, ref DidEquipHandEvent args) => AddScatter(ent);
    private void AddScatter(Entity<XenomorphQueenScreechBlindComponent> ent)
    {
        if (!TryComp<HandsComponent>(ent, out var hands)) return;
        foreach (var name in hands.Hands.Keys)
        {
            if (!_containers.TryGetContainer(ent, name, out var container)) continue;
            foreach (var gun in container.ContainedEntities)
            {
                if (!HasComp<GunComponent>(gun)) continue;
                var scatter = EnsureComp<XenomorphQueenScreechScatterComponent>(gun);
                scatter.Holder = ent;
                Dirty(gun, scatter);
                ent.Comp.ModifiedGuns.Add(gun);
                _guns.RefreshModifiers(gun);
            }
        }
    }
    private void OnUnequip(Entity<XenomorphQueenScreechBlindComponent> ent, ref DidUnequipHandEvent args)
    {
        ent.Comp.ModifiedGuns.Remove(args.Unequipped);
        RemoveScatter(ent, args.Unequipped);
    }
    private void RemoveScatter(EntityUid user, EntityUid gun)
    {
        if (!TryComp<XenomorphQueenScreechScatterComponent>(gun, out var scatter) || scatter.Holder != user) return;
        RemComp<XenomorphQueenScreechScatterComponent>(gun);
        _guns.RefreshModifiers(gun);
    }
    private void OnUnblind(Entity<XenomorphQueenScreechBlindComponent> ent, ref ComponentShutdown args)
    {
        foreach (var gun in ent.Comp.ModifiedGuns) RemoveScatter(ent, gun);
        ent.Comp.ModifiedGuns.Clear();
    }
    private void OnScatter(Entity<XenomorphQueenScreechScatterComponent> ent, ref GunRefreshModifiersEvent args)
    {
        args.MinAngle += Angle.FromDegrees(45);
        args.MaxAngle += Angle.FromDegrees(45);
    }
}
