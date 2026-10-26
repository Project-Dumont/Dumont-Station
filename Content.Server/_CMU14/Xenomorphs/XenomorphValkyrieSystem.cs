using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Server.Atmos.EntitySystems;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Standing;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphValkyrieSystem : EntitySystem
{
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private Content.Shared.Interaction.SharedInteractionSystem _interaction = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private FlammableSystem _fire = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private XenomorphArmorSystem _armor = default!;
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private StandingStateSystem _standing = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedJitteringSystem _jitter = default!;
    [Dependency] private IGameTiming _timing = default!;
    private readonly HashSet<Entity<XenomorphComponent>> _allies = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphValkyrieComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<XenomorphValkyrieComponent, ComponentRemove>(OnRemoved);
        SubscribeLocalEvent<XenomorphValkyrieComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<XenomorphValkyrieComponent, XenomorphFuryAlertEvent>(OnAlert);
        SubscribeLocalEvent<XenomorphValkyrieComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<XenomorphValkyrieComponent, XenomorphTailFountainActionEvent>(OnFountain);
        SubscribeLocalEvent<XenomorphValkyrieComponent, XenomorphTantrumActionEvent>(OnTantrum);
        SubscribeLocalEvent<XenomorphValkyrieComponent, XenomorphFightOrFlightActionEvent>(OnRoar);
        InitializeMovement();
    }

    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid);
    private bool Ally(EntityUid uid) => HasComp<XenomorphComponent>(uid) && !_mobs.IsDead(uid);
    private void OnInit(Entity<XenomorphValkyrieComponent> ent, ref MapInitEvent args) => Refresh(ent);
    private void OnRejuvenate(Entity<XenomorphValkyrieComponent> ent, ref RejuvenateEvent args) { ent.Comp.Fury = 200; Refresh(ent); }
    private void OnAlert(Entity<XenomorphValkyrieComponent> ent, ref XenomorphFuryAlertEvent args)
    {
        if (args.Handled) return;
        _popup.PopupEntity(Loc.GetString("dumont-xenomorph-fury-amount", ("amount", ent.Comp.Fury)), ent, ent);
        args.Handled = true;
    }
    private void Refresh(Entity<XenomorphValkyrieComponent> ent)
    {
        Dirty(ent);
        _alerts.ShowAlert(ent, "DumontXenomorphFury", (short)(18 - Math.Clamp(ent.Comp.Fury * 18 / 200, 0, 18)));
    }
    private bool Spend(Entity<XenomorphValkyrieComponent> ent, int fury, int plasma)
    {
        if (ent.Comp.Fury < fury || !_plasma.HasPlasma(ent, plasma))
        {
            _popup.PopupEntity(Loc.GetString("dumont-xenomorph-valkyrie-resources", ("fury", fury), ("plasma", plasma)), ent, ent);
            return false;
        }
        _plasma.ChangePlasmaAmount(ent, -plasma);
        ent.Comp.Fury -= fury;
        Refresh(ent);
        return true;
    }
    private void OnMelee(Entity<XenomorphValkyrieComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || !args.HitEntities.Any(uid => HasComp<MobStateComponent>(uid) && !HasComp<XenomorphComponent>(uid) && !_mobs.IsDead(uid))) return;
        ent.Comp.Fury = Math.Min(200, ent.Comp.Fury + 15);
        Refresh(ent);
        var amount = HasComp<XenomorphTantrumComponent>(ent) ? 25 : 15;
        _allies.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, 7 / MathF.Sqrt(MathF.PI), _allies);
        foreach (var ally in _allies)
        {
            if (!Ally(ally) || !TryComp<DamageableComponent>(ally, out var damage) || damage.TotalDamage <= 0) continue;
            var heal = new DamageSpecifier();
            foreach (var (type, value) in damage.Damage.DamageDict)
                if (value > 0) heal.DamageDict[type] = -Math.Min(value.Float(), value.Float() / damage.TotalDamage.Float() * amount);
            _damage.TryChangeDamage(ally, heal, ignoreResistances: true, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAll);
            Spawn("DumontXenomorphValkyrieHeal", new EntityCoordinates(ally, Vector2.Zero));
        }
    }
    private void OnFountain(Entity<XenomorphValkyrieComponent> ent, ref XenomorphTailFountainActionEvent args)
    {
        if (args.Handled || !Ready(ent) || args.Target == ent.Owner || !HasComp<MobStateComponent>(args.Target)
            || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, 2)) return;
        _fire.Extinguish(args.Target);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg"), args.Target);
        Spawn("DumontXenomorphAcidExtinguish", new EntityCoordinates(args.Target, Vector2.Zero));
        if (TryComp<MeleeWeaponComponent>(ent, out var melee))
        {
            melee.NextAttack = _timing.CurTime + TimeSpan.FromSeconds(1);
            Dirty(ent, melee);
        }
        var attack = new MeleeAttackEvent(ent);
        RaiseLocalEvent(ent, ref attack);
        args.Handled = true;
    }
    private void Buff(EntityUid uid, int armor, float speed, double duration)
    {
        var buff = EnsureComp<XenomorphTantrumComponent>(uid);
        buff.Armor = armor;
        buff.Speed = speed;
        buff.Until = _timing.CurTime + TimeSpan.FromSeconds(duration);
        Dirty(uid, buff);
        _movement.RefreshMovementSpeedModifiers(uid);
        _armor.RefreshAlert(uid);
        Spawn("DumontXenomorphTantrumEmpower", new EntityCoordinates(uid, Vector2.Zero));
        _audio.PlayPvs(new SoundCollectionSpecifier("DumontXenomorphRoar"), uid);
    }
    private void OnTantrum(Entity<XenomorphValkyrieComponent> ent, ref XenomorphTantrumActionEvent args)
    {
        if (args.Handled || !Ready(ent) || args.Target == ent.Owner || !Ally(args.Target) || HasComp<XenomorphValkyrieComponent>(args.Target)
            || HasComp<XenomorphTantrumComponent>(ent) || HasComp<XenomorphTantrumComponent>(args.Target)
            || !_transform.InRange(ent.Owner, args.Target, 8) || !Spend(ent, 75, 100)) return;
        Buff(ent, 10, 1, 5);
        var fast = Comp<XenomorphComponent>(args.Target).Caste.Id is "Ravager" or "Crusher";
        Buff(args.Target, fast ? 0 : 15, fast ? 1.33f : 1, fast ? 2 : 3);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphTantrumActionEvent) _actions.SetToggled(action.Owner, true);
        args.Handled = true;
    }
    private void OnRoar(Entity<XenomorphValkyrieComponent> ent, ref XenomorphFightOrFlightActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !Spend(ent, 75, 300)) return;
        var strong = ent.Comp.Fury >= 75;
        _allies.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, strong ? 6 : 4, _allies);
        foreach (var ally in _allies)
        {
            if (!Ally(ally)) continue;
            foreach (var effect in new[] { "StatusEffectStunned", "StatusEffectForcedSleeping" }) _status.TryRemoveStatusEffect(ally, effect);
            if (_status.TryEffectsWithComp<MovementModStatusEffectComponent>(ally, out var effects))
                foreach (var effect in effects)
                    if (effect.Comp1.WalkSpeedModifier < 1 || effect.Comp1.SprintSpeedModifier < 1) _status.TryRemoveStatusEffect(ally, MetaData(effect).EntityPrototype!.ID);
            RemCompDeferred<XenomorphDazedComponent>(ally);
            RemComp<KnockedDownComponent>(ally);
            _standing.Stand(ally);
            _jitter.DoJitter(ally, TimeSpan.FromSeconds(1), true, 80, 8, true);
            Spawn("DumontXenomorphValkyrieCleanse", new EntityCoordinates(ally, Vector2.Zero));
        }
        Spawn(strong ? "DumontXenomorphValkyrieScreech" : "DumontXenomorphValkyrieScreechWeak", new EntityCoordinates(ent, Vector2.Zero));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/xenos_roaring.ogg"), ent);
        args.Handled = true;
    }
    private void OnRemoved(Entity<XenomorphValkyrieComponent> ent, ref ComponentRemove args)
    {
        ClearRetrieve(ent);
        _alerts.ClearAlert(ent, "DumontXenomorphFury");
    }
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphValkyrieComponent>();
        while (query.MoveNext(out var uid, out var valkyrie))
        {
            if (_mobs.IsDead(uid) || valkyrie.NextGain > _timing.CurTime) continue;
            valkyrie.NextGain = _timing.CurTime + TimeSpan.FromSeconds(2);
            valkyrie.Fury = Math.Min(200, valkyrie.Fury + 5);
            Refresh((uid, valkyrie));
        }
        var buffs = EntityQueryEnumerator<XenomorphTantrumComponent>();
        while (buffs.MoveNext(out var uid, out var buff))
        {
            if (_timing.CurTime < buff.Until) continue;
            RemComp<XenomorphTantrumComponent>(uid);
            _armor.RefreshAlert(uid);
            foreach (var action in _actions.GetActions(uid))
                if (_actions.GetEvent(action) is XenomorphTantrumActionEvent) _actions.SetToggled(action.Owner, false);
        }
        UpdateRetrieve();
    }
}
