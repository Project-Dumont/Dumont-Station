using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server.Mind;
using Content.Server._White.Xenomorphs.Infection;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._White.Xenomorphs.Acid.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Standing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared._Starlight.VentCrawling;
using Content.Shared.Item;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphAciderSystem : EntitySystem
{
    [Dependency] private Robust.Shared.Physics.Systems.SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedPointLightSystem _light = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private StandingStateSystem _standing = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private XenomorphSpitterSystem _acid = default!;
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private TurfSystem _turf = default!;
    private readonly List<(TimeSpan At, EntityCoordinates Coordinates, EntityUid? Mind, string? Lineage)> _larvae = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphAciderCorrosionComponent, ComponentRemove>(OnCorrosionRemoved);
        SubscribeLocalEvent<XenomorphAciderComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<XenomorphAciderComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<XenomorphAciderComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<XenomorphAciderComponent, MeleeHitEvent>(OnHit);
        SubscribeLocalEvent<XenomorphAciderComponent, XenomorphPrimeAcidActionEvent>(OnPrime);
        SubscribeLocalEvent<XenomorphAciderComponent, XenomorphAciderCorrodeActionEvent>(OnCorrode);
        SubscribeLocalEvent<XenomorphAciderComponent, XenomorphAciderCorrodeDoAfterEvent>(OnCorrodeDone);
        SubscribeLocalEvent<XenomorphAciderComponent, XenomorphAcidReserveAlertEvent>(OnAlert);
    }
    private void OnCorrosionRemoved(Entity<XenomorphAciderCorrosionComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.Effect is { } effect && Exists(effect)) QueueDel(effect);
    }
    public bool SpendAcid(Entity<XenomorphAciderComponent> ent, int amount)
    {
        if (ent.Comp.Acid < amount) return false;
        ent.Comp.Acid -= amount;
        Refresh(ent);
        return true;
    }
    private void OnInit(Entity<XenomorphAciderComponent> ent, ref MapInitEvent args) => Refresh(ent);
    private void OnRemove(Entity<XenomorphAciderComponent> ent, ref ComponentRemove args) => _alerts.ClearAlert(ent, "DumontXenomorphAcidReserve");
    private void OnRejuvenate(Entity<XenomorphAciderComponent> ent, ref RejuvenateEvent args) { ent.Comp.Acid = 1000; Refresh(ent); }
    private void OnAlert(Entity<XenomorphAciderComponent> ent, ref XenomorphAcidReserveAlertEvent args)
    {
        if (args.Handled) return;
        _popup.PopupEntity(Loc.GetString("dumont-xenomorph-acid-reserve-amount", ("amount", ent.Comp.Acid)), ent, ent);
        args.Handled = true;
    }
    private void Refresh(Entity<XenomorphAciderComponent> ent)
    {
        ent.Comp.Acid = Math.Clamp(ent.Comp.Acid, 0, 1000);
        Dirty(ent);
        _alerts.ShowAlert(ent, "DumontXenomorphAcidReserve", (short)(18 - Math.Clamp(ent.Comp.Acid * 18 / 1000, 0, 18)));
    }
    private void OnHit(Entity<XenomorphAciderComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || !_mobs.IsAlive(ent)) return;
        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(hit) || HasComp<XenomorphComponent>(hit) || _mobs.IsDead(hit)) continue;
            _acid.ApplyAcid(hit);
            ent.Comp.GeneratingUntil = _timing.CurTime + TimeSpan.FromSeconds(6);
            if (!TryComp<XenomorphInfectionComponent>(hit, out var infection) || infection.GrowthStage < infection.MaxGrowthStage)
                ent.Comp.Acid += _standing.IsDown(hit) ? 8 : 14;
            Refresh(ent);
            break;
        }
    }
    private void OnPrime(Entity<XenomorphAciderComponent> ent, ref XenomorphPrimeAcidActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanAttack(ent)) return;
        if (_containers.IsEntityInContainer(ent))
        {
            _popup.PopupEntity(Loc.GetString("dumont-xenomorph-acider-container"), ent, ent);
            return;
        }
        if (ent.Comp.Primed)
        {
            ent.Comp.Acid -= ent.Comp.Acid / 4;
            SetPrimed(ent, false);
            _popup.PopupEntity(Loc.GetString("dumont-xenomorph-acider-cancel"), ent, ent);
        }
        else
        {
            if (ent.Comp.Acid < 200)
            {
                _popup.PopupEntity(Loc.GetString("dumont-xenomorph-acider-not-enough"), ent, ent);
                return;
            }
            ent.Comp.PrimeAction = args.Action.Owner;
            ent.Comp.DetonateAt = _timing.CurTime + TimeSpan.FromSeconds(20);
            ent.Comp.NextCountdown = _timing.CurTime;
            SetPrimed(ent, true);
            _popup.PopupEntity(Loc.GetString("dumont-xenomorph-acider-prime"), ent, ent, PopupType.LargeCaution);
        }
        args.Handled = true;
    }
    private void SetPrimed(Entity<XenomorphAciderComponent> ent, bool primed)
    {
        ent.Comp.Primed = primed;
        _light.SetEnabled(ent, primed);
        if (ent.Comp.PrimeAction is { } action) _actions.SetToggled(action, primed);
        if (primed) RemComp<VentCrawlerComponent>(ent);
        else EnsureComp<VentCrawlerComponent>(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
        Refresh(ent);
    }
    private bool Corrodible(EntityUid target)
        => !HasComp<MobStateComponent>(target)
            && (HasComp<ItemComponent>(target) || HasComp<XenomorphSprayAcidableComponent>(target))
            && !HasComp<AcidCorrodingComponent>(target) && !HasComp<XenomorphAciderCorrosionComponent>(target);
    private void OnCorrode(Entity<XenomorphAciderComponent> ent, ref XenomorphAciderCorrodeActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanInteract(ent, null) || ent.Comp.Acid < 100
            || !Corrodible(args.Target) || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 2)) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(1), new XenomorphAciderCorrodeDoAfterEvent(), ent, args.Target)
        {
            BreakOnMove = true,
            NeedHand = false,
            DistanceThreshold = 2,
        });
    }
    private void OnCorrodeDone(Entity<XenomorphAciderComponent> ent, ref XenomorphAciderCorrodeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target || !Corrodible(target)
            || !_mobs.IsAlive(ent) || ent.Comp.Acid < 100 || !_interaction.InRangeUnobstructed(ent.Owner, target, range: 2)) return;
        ent.Comp.Acid -= 100;
        Refresh(ent);
        Corrode(target, 20, 20, true);
        args.Handled = true;
    }
    public void Corrode(EntityUid target, float damage, float duration, bool strong, string? prototype = null)
    {
        if (HasComp<AcidCorrodingComponent>(target) || HasComp<XenomorphAciderCorrosionComponent>(target)) return;
        var coating = AddComp<XenomorphAciderCorrosionComponent>(target);
        coating.Effect = Spawn(prototype ?? (strong ? "DumontXenomorphAciderStrongAcid" : "DumontXenomorphCorrosiveAcidEffect"), new EntityCoordinates(target, Vector2.Zero));
        coating.Damage = damage;
        coating.ExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(duration);
        coating.NextDamage = _timing.CurTime;
    }

    private void Detonate(Entity<XenomorphAciderComponent> ent)
    {
        var origin = _transform.GetMoverCoordinates(ent);
        var energy = ent.Comp.Acid;
        var acidRange = (energy / 200f * 2 + 1) / MathF.Sqrt(MathF.PI);
        var burnRange = (energy / 100f * 2 + 1) / MathF.Sqrt(MathF.PI);
        foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(origin, burnRange))
        {
            if (HasComp<XenomorphComponent>(target) || _mobs.IsDead(target)
                || !_interaction.InRangeUnobstructed(ent.Owner, target.Owner, range: burnRange)) continue;
            if (!origin.TryDistance(EntityManager, Transform(target).Coordinates, out var distance)) continue;
            _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Caustic"] = (burnRange - distance) * energy / 5f / burnRange } }, ignoreResistances: true, origin: ent);
            if (distance <= acidRange) _acid.ApplyAcid(target);
        }
        foreach (var target in _lookup.GetEntitiesInRange<XenomorphSprayAcidableComponent>(origin, acidRange))
            if (_interaction.InRangeUnobstructed(ent.Owner, target.Owner, range: acidRange)) Corrode(target, 8, 255, false);
        if (Transform(ent).GridUid is { } grid)
        {
            var center = _transform.ToCoordinates(grid, _transform.GetMapCoordinates(ent)).Position;
            for (var x = (int)MathF.Floor(center.X - acidRange); x <= MathF.Ceiling(center.X + acidRange); x++)
            for (var y = (int)MathF.Floor(center.Y - acidRange); y <= MathF.Ceiling(center.Y + acidRange); y++)
            {
                var tile = new EntityCoordinates(grid, x + .5f, y + .5f);
                if (!_interaction.InRangeUnobstructed(ent.Owner, tile, range: acidRange)) continue;
                if (_turf.GetTileRef(tile) is not { } turf || _turf.IsTileBlocked(turf, Content.Shared.Physics.CollisionGroup.Impassable)) continue;
                Spawn("DumontXenomorphRunnerAcidSmoke", tile);
            }
        }
        EntityUid? mindId = _mind.TryGetMind(ent, out var mind, out _) ? mind : null;
        string? lineage = TryComp<XenomorphLineageComponent>(ent, out var path) ? path.Lineage.Id : null;
        _larvae.Add((_timing.CurTime + TimeSpan.FromSeconds(.5), origin, mindId, lineage));
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/blobattack.ogg"), ent);
        SetPrimed(ent, false);
        _body.GibBody(ent);
    }
    public override void Update(float frameTime)
    {
        var now = _timing.CurTime;
        var corrosion = EntityQueryEnumerator<XenomorphAciderCorrosionComponent>();
        while (corrosion.MoveNext(out var target, out var coating))
        {
            if (now >= coating.ExpiresAt) { QueueDel(target); continue; }
            if (now < coating.NextDamage) continue;
            coating.NextDamage = now + TimeSpan.FromSeconds(1);
            _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Caustic"] = coating.Damage } });
        }
        for (var i = _larvae.Count - 1; i >= 0; i--)
        {
            var larva = _larvae[i];
            if (now < larva.At) continue;
            _larvae.RemoveAt(i);
            if (!Exists(larva.Coordinates.EntityId)) continue;
            var spawned = Spawn("MobXenomorphLarva", larva.Coordinates);
            if (larva.Lineage is { } lineage) EnsureComp<XenomorphLineageComponent>(spawned).Lineage = lineage;
            if (larva.Mind is { } mind && TryComp<MindComponent>(mind, out var mindComp))
            {
                _mind.TransferTo(mind, spawned);
                _mind.UnVisit(mind);
                mindComp.OriginalOwnedEntity = GetNetEntity(spawned);
                Dirty(mind, mindComp);
            }
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/xeno_newlarva.ogg"), spawned);
        }
        var query = EntityQueryEnumerator<XenomorphAciderComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_mobs.IsDead(uid))
            {
                if (comp.Primed) SetPrimed((uid, comp), false);
                continue;
            }
            if (now >= comp.NextGain)
            {
                comp.NextGain = now + TimeSpan.FromSeconds(2);
                if (comp.Acid < 400) comp.Acid++;
                if (now <= comp.GeneratingUntil) comp.Acid++;
                Refresh((uid, comp));
            }
            if (!comp.Primed || now < comp.NextCountdown) continue;
            comp.NextCountdown += TimeSpan.FromSeconds(1);
            var seconds = Math.Max(0, (int)Math.Ceiling((comp.DetonateAt - now).TotalSeconds));
            if (seconds == 0) { Detonate((uid, comp)); continue; }
            _popup.PopupEntity(seconds.ToString(), uid, uid, seconds <= 10 ? PopupType.LargeCaution : PopupType.MediumCaution);
            if (seconds % 2 == 0 && seconds > 1)
                _audio.PlayPvs(new SoundPathSpecifier(seconds % 4 == 0 ? "/Audio/_RMC14/Xeno/runner_charging_1.ogg" : "/Audio/_RMC14/Xeno/runner_charging_2.ogg"), uid,
                    AudioParams.Default.WithVolume(-3 + 23 * (1 - seconds / 20f)));
        }
    }
}

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphAciderCorrosionComponent : Component
{
    [DataField(customTypeSerializer: typeof(Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.TimeOffsetSerializer)), AutoPausedField] public TimeSpan ExpiresAt;
    [DataField(customTypeSerializer: typeof(Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextDamage;
    [DataField] public float Damage;
    public EntityUid? Effect;
}
