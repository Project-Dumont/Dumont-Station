using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Mind;
using Content.Server.Atmos.EntitySystems;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphHealerRebirthComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan At;
    public EntityCoordinates Coordinates;
    public ProtoId<XenomorphLineagePrototype> Lineage;
}

public sealed partial class XenomorphHealerSystem : EntitySystem
{
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private MobThresholdSystem _thresholds = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private Content.Shared.Interaction.SharedInteractionSystem _interaction = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private FlammableSystem _fire = default!;
    [Dependency] private StatusEffectsSystem _status = default!;
    [Dependency] private StandingStateSystem _standing = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphHealerComponent, XenomorphSacrificeActionEvent>(OnSacrifice);
        SubscribeLocalEvent<XenomorphHealerComponent, XenomorphHealerPlasmaActionEvent>(OnPlasma);
        SubscribeLocalEvent<XenomorphHealerComponent, XenomorphHealerPlasmaDoAfterEvent>(OnPlasmaTransferred);
        SubscribeLocalEvent<XenomorphHealerComponent, XenomorphHealingReserveAlertEvent>(OnAlert);
        SubscribeLocalEvent<XenomorphHealerComponent, ComponentRemove>(OnRemoved);
    }

    private void OnRemoved(Entity<XenomorphHealerComponent> ent, ref ComponentRemove args)
        => _alerts.ClearAlert(ent, "DumontXenomorphHealingReserve");

    private void OnAlert(Entity<XenomorphHealerComponent> ent, ref XenomorphHealingReserveAlertEvent args)
    {
        if (args.Handled) return;
        _popup.PopupEntity(Loc.GetString("xenomorph-healer-reserve", ("amount", ent.Comp.Transferred), ("max", ent.Comp.MaxTransferred)), ent, ent);
        args.Handled = true;
    }

    public void Heal(EntityUid uid, FixedPoint2 amount)
    {
        if (!TryComp<DamageableComponent>(uid, out var damage)) return;
        var heal = new DamageSpecifier();
        foreach (var type in new[] { "Blunt", "Slash", "Piercing", "Heat", "Shock", "Cold", "Caustic" })
        {
            if (amount <= 0 || !damage.Damage.DamageDict.TryGetValue(type, out var value) || value <= 0) continue;
            var restored = FixedPoint2.Min(value, amount);
            heal.DamageDict[type] = -restored;
            amount -= restored;
        }
        _damage.TryChangeDamage(uid, heal, ignoreResistances: true, targetPart: TargetBodyPart.All,
                        splitDamage: SplitDamageBehavior.SplitEnsureAll);
    }

    private void OnSacrifice(Entity<XenomorphHealerComponent> ent, ref XenomorphSacrificeActionEvent args)
    {
        if (args.Handled || ent.Owner == args.Target || !_mobs.IsAlive(ent) || !_blocker.CanConsciouslyPerformAction(ent)
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting
            || !HasComp<XenomorphLineageComponent>(args.Target) || !TryComp<XenomorphComponent>(args.Target, out var target)
            || target.Caste == "Larva" || _mobs.IsDead(args.Target) || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, 2)
            || !TryComp<DamageableComponent>(args.Target, out var injury) || injury.TotalDamage <= 0
            || !TryComp<DamageableComponent>(ent, out var own)
            || !_thresholds.TryGetThresholdForState(args.Target, MobState.Critical, out var critical)
            || !_thresholds.TryGetThresholdForState(ent, MobState.Dead, out var dead)) return;
        _fire.Extinguish(args.Target);
        var remaining = FixedPoint2.Max(0, dead.Value - own.TotalDamage);
        var heal = FixedPoint2.Max(0, injury.TotalDamage - critical.Value) + remaining * 0.75;
        Heal(args.Target, heal);
        foreach (var effect in new[] { "StatusEffectStunned", "StatusEffectForcedSleeping", "XenomorphStompSlowdown" })
            _status.TryRemoveStatusEffect(args.Target, effect);
        RemCompDeferred<XenomorphDazedComponent>(args.Target);
        RemComp<KnockedDownComponent>(args.Target);
        _standing.Stand(args.Target);
        Spawn("DumontXenomorphSacrificeHealEffect", new EntityCoordinates(args.Target, Vector2.Zero));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_roar1.ogg"), ent);
        _popup.PopupEntity(Loc.GetString("xenomorph-healer-sacrifice"), args.Target, args.Target, PopupType.Large);
        if (_mind.TryGetMind(ent, out var mindUid, out var mind))
        {
            if (ent.Comp.Transferred >= ent.Comp.MaxTransferred)
            {
                var rebirth = EnsureComp<XenomorphHealerRebirthComponent>(mindUid);
                rebirth.At = _timing.CurTime + TimeSpan.FromSeconds(0.5);
                rebirth.Coordinates = _transform.GetMoverCoordinates(ent);
                rebirth.Lineage = Comp<XenomorphLineageComponent>(ent).Lineage;
                _popup.PopupEntity(Loc.GetString("xenomorph-healer-rebirth-ready"), ent, ent);
            }
            _mind.TransferTo(mindUid, null, mind: mind);
        }
        args.Handled = true;
        _body.GibBody(ent);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphHealerComponent>();
        while (query.MoveNext(out var uid, out var healer))
        {
            if (healer.Displayed == healer.Transferred) continue;
            if (healer.Displayed >= 0 && healer.Transferred >= healer.MaxTransferred)
                _popup.PopupEntity(Loc.GetString("xenomorph-healer-rebirth-ready"), uid, uid, PopupType.Large);
            healer.Displayed = healer.Transferred;
            _alerts.ShowAlert(uid, "DumontXenomorphHealingReserve", (short)(18 - Math.Clamp(healer.Transferred * 18 / Math.Max(1, healer.MaxTransferred), 0, 18)));
        }
        var rebirths = EntityQueryEnumerator<XenomorphHealerRebirthComponent, MindComponent>();
        while (rebirths.MoveNext(out var uid, out var rebirth, out var mind))
        {
            if (_timing.CurTime < rebirth.At) continue;
            RemCompDeferred<XenomorphHealerRebirthComponent>(uid);
            if (!Exists(rebirth.Coordinates.EntityId) || mind.OwnedEntity is { } owned && Exists(owned) && !HasComp<GhostComponent>(owned)) continue;
            var larva = Spawn("MobXenomorphLarva", rebirth.Coordinates);
            EnsureComp<XenomorphLineageComponent>(larva).Lineage = rebirth.Lineage;
            _mind.TransferTo(uid, larva, mind: mind);
            _mind.UnVisit(uid, mind);
            mind.OriginalOwnedEntity = GetNetEntity(larva);
            Dirty(uid, mind);
            _popup.PopupEntity(Loc.GetString("xenomorph-healer-reborn"), larva, larva, PopupType.Medium);
        }
    }
}
