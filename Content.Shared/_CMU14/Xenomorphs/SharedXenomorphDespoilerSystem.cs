using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Damage;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.GameStates;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphDespoilerSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphDespoilerComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<XenomorphDespoilerComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<XenomorphDespoilerComponent, GetMeleeDamageEvent>(OnDamage);
    }
    private void OnSpeed(Entity<XenomorphDespoilerComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Charging)
            args.ModifySpeed(0.4f, 0.4f);
    }
    private void OnState(Entity<XenomorphDespoilerComponent> ent, ref AfterAutoHandleStateEvent args)
        => _movement.RefreshMovementSpeedModifiers(ent);
    private void OnDamage(Entity<XenomorphDespoilerComponent> ent, ref GetMeleeDamageEvent args)
        => args.Damage += new DamageSpecifier { DamageDict = { ["Heat"] = ent.Comp.Stacks * 20 } };
}
