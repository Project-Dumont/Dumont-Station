using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Rejuvenate;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Player;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphToxicSystem
{
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private StandingStateSystem _standing = default!;
    private void InitializeIntoxication()
    {
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, ComponentRemove>(OnIntoxicatedRemoved);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, MobStateChangedEvent>(OnMobState);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, XenomorphIntoxicatedResistAlertEvent>(OnResist);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, XenomorphIntoxicatedResistDoAfterEvent>(OnResistDone);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, DropAttemptEvent>(OnDropAttempt);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, PickupAttemptEvent>(OnPickupAttempt);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, ThrowAttemptEvent>(OnThrowAttempt);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, UseAttemptEvent>(OnUseAttempt);
        SubscribeLocalEvent<XenomorphIntoxicatedComponent, InteractionAttemptEvent>(OnInteractionAttempt);
    }
    public bool AddIntoxication(EntityUid source, EntityUid target, int stacks)
    {
        if (!Victim(target) || stacks <= 0) return false;
        var toxin = EnsureComp<XenomorphIntoxicatedComponent>(target);
        toxin.Stacks = Math.Clamp(toxin.Stacks + stacks, 0, 30);
        toxin.LastSource = source;
        if (toxin.NextTick == TimeSpan.Zero) toxin.NextTick = _timing.CurTime + TimeSpan.FromSeconds(2);
        RefreshIntoxication((target, toxin));
        _alerts.ShowAlert(target, "DumontXenomorphIntoxicated");
        _flash.RaiseEffect(Color.FromHex("#7DCC00"), new List<EntityUid> { target }, Filter.Pvs(target, entityManager: EntityManager));
        _popup.PopupEntity(Loc.GetString("dumont-xenomorph-intoxicated-gained"), target, target);
        return true;
    }
    private void RefreshIntoxication(Entity<XenomorphIntoxicatedComponent> ent)
    {
        Dirty(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
    }
    private void OnIntoxicatedRemoved(Entity<XenomorphIntoxicatedComponent> ent, ref ComponentRemove args)
    {
        _alerts.ClearAlert(ent, "DumontXenomorphIntoxicated");
        StopResist(ent);
    }
    private void OnRejuvenate(Entity<XenomorphIntoxicatedComponent> ent, ref RejuvenateEvent args) => RemCompDeferred<XenomorphIntoxicatedComponent>(ent);
    private void OnMobState(Entity<XenomorphIntoxicatedComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead) RemCompDeferred<XenomorphIntoxicatedComponent>(ent);
    }
    private void OnResist(Entity<XenomorphIntoxicatedComponent> ent, ref XenomorphIntoxicatedResistAlertEvent args)
    {
        if (args.Handled || ent.Comp.Resisting || !_blocker.CanInteract(ent, null)) return;
        if (!_doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, 3, new XenomorphIntoxicatedResistDoAfterEvent(), ent, ent)
        { BreakOnMove = true, BreakOnDamage = false, NeedHand = false, RequireCanInteract = false, MovementThreshold = .05f })) return;
        ent.Comp.WasDown = _standing.IsDown(ent);
        ent.Comp.ForcedDown = !ent.Comp.WasDown && _standing.Down(ent, dropHeldItems: false, force: true);
        ent.Comp.Resisting = true;
        var animation = EnsureComp<XenomorphSweepingComponent>(ent);
        animation.TotalRotations = -8;
        animation.Delay = TimeSpan.FromSeconds(.25);
        animation.NextRotation = _timing.CurTime;
        Dirty(ent, animation);
        Dirty(ent);
        args.Handled = true;
    }
    private void StopResist(Entity<XenomorphIntoxicatedComponent> ent)
    {
        if (ent.Comp.ForcedDown && !ent.Comp.WasDown && !HasComp<KnockedDownComponent>(ent) && !_mobs.IsDead(ent) && !TerminatingOrDeleted(ent))
            _standing.Stand(ent, force: true);
        if (ent.Comp.Resisting) RemCompDeferred<XenomorphSweepingComponent>(ent);
        ent.Comp.Resisting = false;
        ent.Comp.ForcedDown = false;
        if (ent.Comp.Running) Dirty(ent);
    }
    private void OnResistDone(Entity<XenomorphIntoxicatedComponent> ent, ref XenomorphIntoxicatedResistDoAfterEvent args)
    {
        if (args.Handled) return;
        StopResist(ent);
        args.Handled = true;
        if (args.Cancelled) return;
        ent.Comp.Stacks = Math.Max(0, ent.Comp.Stacks - 8);
        RefreshIntoxication(ent);
        if (ent.Comp.Stacks == 0) RemCompDeferred<XenomorphIntoxicatedComponent>(ent);
    }
    private void OnAttackAttempt(Entity<XenomorphIntoxicatedComponent> ent, ref AttackAttemptEvent args) { if (ent.Comp.Resisting) args.Cancel(); }
    private void OnDropAttempt(Entity<XenomorphIntoxicatedComponent> ent, ref DropAttemptEvent args) { if (ent.Comp.Resisting) args.Cancel(); }
    private void OnPickupAttempt(Entity<XenomorphIntoxicatedComponent> ent, ref PickupAttemptEvent args) { if (ent.Comp.Resisting) args.Cancel(); }
    private void OnThrowAttempt(Entity<XenomorphIntoxicatedComponent> ent, ref ThrowAttemptEvent args) { if (ent.Comp.Resisting) args.Cancel(); }
    private void OnUseAttempt(Entity<XenomorphIntoxicatedComponent> ent, ref UseAttemptEvent args) { if (ent.Comp.Resisting) args.Cancel(); }
    private void OnInteractionAttempt(Entity<XenomorphIntoxicatedComponent> ent, ref InteractionAttemptEvent args) { if (ent.Comp.Resisting) args.Cancelled = true; }
    private void UpdateIntoxication()
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<XenomorphIntoxicatedComponent>();
        while (query.MoveNext(out var uid, out var toxin))
        {
            if (now < toxin.NextTick) continue;
            toxin.NextTick = now + TimeSpan.FromSeconds(2);
            var amount = 1 + MathF.Round(toxin.Stacks / 10f, MidpointRounding.AwayFromZero);
            _damage.TryChangeDamage(uid, new DamageSpecifier { DamageDict = { ["Caustic"] = amount } }, origin: toxin.LastSource is { } source && Exists(source) ? source : null);
            toxin.Stacks = Math.Max(0, toxin.Stacks - 1);
            RefreshIntoxication((uid, toxin));
            if (toxin.Stacks == 0) RemCompDeferred<XenomorphIntoxicatedComponent>(uid);
        }
    }
}
