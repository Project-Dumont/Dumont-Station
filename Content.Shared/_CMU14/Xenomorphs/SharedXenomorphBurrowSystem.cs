using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Events;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Events;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class SharedXenomorphBurrowSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphBurrowComponent, UpdateCanMoveEvent>(OnMove);
        SubscribeLocalEvent<XenomorphBurrowComponent, AttackAttemptEvent>(OnAttack);
        SubscribeLocalEvent<XenomorphBurrowComponent, InteractionAttemptEvent>(OnInteract);
        SubscribeLocalEvent<XenomorphBurrowComponent, BeforeDamageChangedEvent>(OnDamage);
        SubscribeLocalEvent<XenomorphBurrowComponent, BeforeStatusEffectAddedEvent>(OnStatus);
        SubscribeLocalEvent<XenomorphBurrowComponent, KnockDownAttemptEvent>(OnKnockdown);
        SubscribeLocalEvent<XenomorphBurrowComponent, PreventCollideEvent>(OnCollide);
        SubscribeLocalEvent<XenomorphBurrowComponent, ExamineAttemptEvent>(OnExamine);
        SubscribeLocalEvent<XenomorphBurrowComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnMove(Entity<XenomorphBurrowComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.Active || ent.Comp.BuildingTunnel)
            args.Cancel();
    }

    private void OnAttack(Entity<XenomorphBurrowComponent> ent, ref AttackAttemptEvent args)
    {
        if (ent.Comp.Active) args.Cancel();
    }

    private void OnInteract(Entity<XenomorphBurrowComponent> ent, ref InteractionAttemptEvent args)
    {
        if (ent.Comp.Active) args.Cancelled = true;
    }

    private void OnDamage(Entity<XenomorphBurrowComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (ent.Comp.Active) args.Cancelled = true;
    }

    private void OnStatus(Entity<XenomorphBurrowComponent> ent, ref BeforeStatusEffectAddedEvent args)
    {
        if (ent.Comp.Active) args.Cancelled = true;
    }

    private void OnKnockdown(Entity<XenomorphBurrowComponent> ent, ref KnockDownAttemptEvent args)
    {
        if (ent.Comp.Active) args.Cancelled = true;
    }

    private void OnCollide(Entity<XenomorphBurrowComponent> ent, ref PreventCollideEvent args)
    {
        if (ent.Comp.Active) args.Cancelled = true;
    }

    private void OnExamine(Entity<XenomorphBurrowComponent> ent, ref ExamineAttemptEvent args)
    {
        if (ent.Comp.Active && args.Examiner != ent.Owner) args.Cancel();
    }

    private void OnState(Entity<XenomorphBurrowComponent> ent, ref AfterAutoHandleStateEvent args)
        => _blocker.UpdateCanMove(ent);
}
