using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;
using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphFruitSystem
{
    [Dependency] private XenomorphVanguardSystem _vanguard = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private MobThresholdSystem _thresholds = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    private void OnChoose(Entity<XenomorphFruitPlanterComponent> ent, ref XenomorphChooseFruitActionEvent args)
    {
        if (args.Handled) return;
        UpdateFruitUi(ent);
        args.Handled = _ui.TryOpenUi(ent.Owner, XenomorphFruitUiKey.Choose, ent.Owner);
    }

    private void UpdateFruitUi(Entity<XenomorphFruitPlanterComponent> ent)
    {
        if (!_ui.HasUi(ent.Owner, XenomorphFruitUiKey.Choose)) return;
        ent.Comp.Fruit.RemoveAll(uid => !Exists(uid) || EntityManager.IsQueuedForDeletion(uid));
        _ui.SetUiState(ent.Owner, XenomorphFruitUiKey.Choose, new XenomorphFruitUiState(ent.Comp.Fruit.Count, ent.Comp.Maximum));
    }

    private void OnChosen(Entity<XenomorphFruitPlanterComponent> ent, ref XenomorphChooseFruitMessage args)
    {
        if (!ent.Comp.Allowed.Contains(args.Kind)) return;
        ent.Comp.Choice = args.Kind;
        Dirty(ent);
        var state = "fruit_" + args.Kind.ToString().ToLowerInvariant();
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphPlantFruitActionEvent)
                _actions.SetIcon(action.AsNullable(), new SpriteSpecifier.Rsi(new ResPath("_RMC14/Structures/Xenos/xeno_fruit.rsi"), state));
    }

    private void ApplyFruit(XenomorphFruitKind kind, EntityUid target)
    {
        if (kind == XenomorphFruitKind.Lesser) { HealFruit(target, 50); return; }
        var buff = EnsureComp<XenomorphFruitBuffComponent>(target);
        switch (kind)
        {
            case XenomorphFruitKind.Greater:
                HealFruit(target, 75);
                buff.HealTicks = 5;
                buff.HealPerTick = 20;
                if (buff.NextHeal <= _timing.CurTime) buff.NextHeal = _timing.CurTime + TimeSpan.FromSeconds(1);
                break;
            case XenomorphFruitKind.Plasma:
                buff.PlasmaTicks = 5;
                if (buff.NextPlasma <= _timing.CurTime) buff.NextPlasma = _timing.CurTime + TimeSpan.FromSeconds(3);
                break;
            case XenomorphFruitKind.Speed:
                buff.SpeedBonus = 0.2f;
                buff.SpeedUntil = _timing.CurTime + TimeSpan.FromSeconds(15);
                Dirty(target, buff);
                _movement.RefreshMovementSpeedModifiers(target);
                break;
            case XenomorphFruitKind.Spore:
                buff.SporeUntil = _timing.CurTime + TimeSpan.FromSeconds(90);
                break;
            case XenomorphFruitKind.Unstable:
                if (TryComp<XenomorphPraetorianComponent>(target, out var praetorian))
                {
                    praetorian.Shield = 0;
                    Dirty(target, praetorian);
                }
                if (TryComp<XenomorphVanguardComponent>(target, out var vanguard) && vanguard.Shield > 0)
                    _vanguard.RemoveShield((target, vanguard));
                buff.HealTicks = 15;
                buff.HealPerTick = 5;
                if (buff.NextHeal <= _timing.CurTime) buff.NextHeal = _timing.CurTime + TimeSpan.FromSeconds(1);
                if (_thresholds.TryGetThresholdForState(target, MobState.Dead, out var dead))
                {
                    var maximum = Math.Min(200, dead.Value.Float() * 0.3f);
                    if (buff.Shield <= 0) buff.ShieldDecayAt = _timing.CurTime + TimeSpan.FromSeconds(60);
                    buff.Shield = Math.Min(maximum, buff.Shield + maximum);
                    if (!Exists(buff.ShieldEffect)) buff.ShieldEffect = Spawn("DumontXenomorphFruitShield", new EntityCoordinates(target, Vector2.Zero));
                }
                break;
        }
    }

    private void OnBuffMelee(Entity<XenomorphFruitBuffComponent> ent, ref MeleeHitEvent args)
    {
        if (args.Handled || !args.IsHit || args.User != ent.Owner || ent.Comp.SporeUntil <= _timing.CurTime
            || ent.Comp.SporeReduction >= 0.25f || !args.HitEntities.Any(uid => !HasComp<XenomorphComponent>(uid)
                && HasComp<Content.Shared.Mobs.Components.MobStateComponent>(uid) && !_mobs.IsDead(uid))) return;
        var oldReduction = ent.Comp.SporeReduction;
        ent.Comp.SporeReduction = Math.Min(0.25f, oldReduction + 0.05f);
        foreach (var action in _actions.GetActions(ent))
        {
            if (MetaData(action.Owner).EntityPrototype?.ID.StartsWith("ActionXenomorph", StringComparison.Ordinal) != true)
                continue;
            if (action.Comp.UseDelay is { } delay && delay > TimeSpan.Zero)
            {
                ent.Comp.ActionDelays.TryAdd(action.Owner, delay);
                _actions.SetUseDelay(action.AsNullable(), ent.Comp.ActionDelays[action.Owner] * (1 - ent.Comp.SporeReduction));
            }
            if (action.Comp.Cooldown is { } cooldown && cooldown.End > _timing.CurTime)
            {
                var remaining = (cooldown.End - _timing.CurTime) * ((1 - ent.Comp.SporeReduction) / (1 - oldReduction));
                _actions.SetCooldown(action.AsNullable(), cooldown.Start, _timing.CurTime + remaining);
            }
        }
    }

    private void RestoreDelays(Entity<XenomorphFruitBuffComponent> ent)
    {
        foreach (var (uid, delay) in ent.Comp.ActionDelays)
            if (Exists(uid)) _actions.SetUseDelay(uid, delay);
        ent.Comp.ActionDelays.Clear();
        ent.Comp.SporeReduction = 0;
        ent.Comp.SporeUntil = TimeSpan.Zero;
    }

    private void OnBuffDamage(Entity<XenomorphFruitBuffComponent> ent, ref DamageModifyEvent args)
    {
        if (ent.Comp.Shield <= 0 || args.Damage.GetTotal() <= 0) return;
        args.Damage = new DamageSpecifier(args.Damage);
        foreach (var (type, value) in args.Damage.DamageDict)
        {
            if (value <= 0) continue;
            var absorbed = Math.Min(value.Float(), ent.Comp.Shield);
            ent.Comp.Shield -= absorbed;
            args.Damage.DamageDict[type] -= Content.Goobstation.Maths.FixedPoint.FixedPoint2.New(absorbed);
        }
        if (ent.Comp.Shield <= 0)
        {
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Bullets/shield_break_c1.ogg"), ent);
            ClearFruitShield(ent);
        }
    }

    private void ClearFruitShield(Entity<XenomorphFruitBuffComponent> ent)
    {
        ent.Comp.Shield = 0;
        if (Exists(ent.Comp.ShieldEffect)) QueueDel(ent.Comp.ShieldEffect);
        ent.Comp.ShieldEffect = null;
    }

    private void OnBuffRemoved(Entity<XenomorphFruitBuffComponent> ent, ref ComponentRemove args)
    {
        ClearFruitShield(ent);
        RestoreDelays(ent);
    }

    private void UpdateBuffs(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphFruitBuffComponent>();
        while (query.MoveNext(out var uid, out var buff))
        {
            if (buff.SpeedBonus > 0 && _timing.CurTime >= buff.SpeedUntil)
            {
                buff.SpeedBonus = 0;
                Dirty(uid, buff);
                _movement.RefreshMovementSpeedModifiers(uid);
                _popup.PopupEntity(Loc.GetString("xenomorph-fruit-buff-end"), uid, uid);
            }
            if (buff.SporeUntil != TimeSpan.Zero && _timing.CurTime >= buff.SporeUntil) RestoreDelays((uid, buff));
            if (buff.HealTicks > 0 && _timing.CurTime >= buff.NextHeal)
            {
                if (!_mobs.IsDead(uid)) HealFruit(uid, buff.HealPerTick);
                buff.HealTicks--;
                buff.NextHeal = _timing.CurTime + TimeSpan.FromSeconds(1);
            }
            if (buff.PlasmaTicks > 0 && _timing.CurTime >= buff.NextPlasma)
            {
                _plasma.ChangePlasmaAmount(uid, 48);
                buff.PlasmaTicks--;
                buff.NextPlasma = _timing.CurTime + TimeSpan.FromSeconds(3);
            }
            if (buff.Shield > 0 && _timing.CurTime >= buff.ShieldDecayAt)
            {
                buff.Shield -= 10 * frameTime;
                if (buff.Shield <= 0) ClearFruitShield((uid, buff));
            }
            if (buff.SpeedBonus <= 0 && buff.SporeUntil == TimeSpan.Zero && buff.HealTicks <= 0
                && buff.PlasmaTicks <= 0 && buff.Shield <= 0) RemCompDeferred<XenomorphFruitBuffComponent>(uid);
        }
    }
}
