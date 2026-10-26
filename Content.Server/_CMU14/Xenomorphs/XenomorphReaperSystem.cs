using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Linq;
using System.Numerics;
using Content.Server.Atmos.Rotting;
using Content.Server.DoAfter;
using Content.Server._White.Xenomorphs.FaceHugger;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Traits.Assorted;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphFleshHarvestedComponent : Component;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenomorphReaperFieldComponent : Component
{
    [DataField] public bool Bloom;
    public EntityUid Reaper;
    [AutoPausedField] public TimeSpan NextPulse;
}

public sealed partial class XenomorphReaperSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private RottingSystem _rotting = default!;
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private XenomorphArmorSystem _armor = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphReaperComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<XenomorphReaperComponent, ComponentRemove>(OnRemoved);
        SubscribeLocalEvent<XenomorphReaperComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<XenomorphReaperComponent, XenomorphFleshAlertEvent>(OnAlert);
        SubscribeLocalEvent<XenomorphReaperComponent, XenomorphFleshHarvestActionEvent>(OnHarvest);
        SubscribeLocalEvent<XenomorphReaperComponent, XenomorphFleshHarvestDoAfterEvent>(OnHarvested);
        SubscribeLocalEvent<XenomorphReaperComponent, XenomorphRaptureActionEvent>(OnRapture);
        SubscribeLocalEvent<XenomorphReaperComponent, XenomorphFleshBloomActionEvent>(OnBloom);
        SubscribeLocalEvent<XenomorphReaperComponent, XenomorphFleshBloomDoAfterEvent>(OnBloomed);
        SubscribeLocalEvent<XenomorphReaperComponent, XenomorphRedGasActionEvent>(OnGas);
        SubscribeLocalEvent<XenomorphReaperComponent, XenomorphRedGasDoAfterEvent>(OnGasStep);
        SubscribeLocalEvent<XenomorphReaperComponent, XenomorphCarrionMantleActionEvent>(OnMantle);
        SubscribeLocalEvent<XenomorphCarrionMantleComponent, DamageModifyEvent>(OnShieldDamage,
            after: new[] { typeof(XenomorphArmorSystem), typeof(XenomorphCrusherSystem) });
        SubscribeLocalEvent<XenomorphCarrionMantleComponent, ComponentRemove>(OnMantleRemoved);
    }

    private bool CanUse(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanInteract(uid, null)
        && !_containers.IsEntityInContainer(uid)
        && (!TryComp<XenomorphRestComponent>(uid, out var rest) || !rest.Resting);

    private bool Friendly(EntityUid uid) => HasComp<XenomorphComponent>(uid) || HasComp<FaceHuggerComponent>(uid);
    private bool Enemy(EntityUid uid) => HasComp<MobStateComponent>(uid) && !Friendly(uid) && !_mobs.IsDead(uid)
        && !_containers.IsEntityInContainer(uid);

    private void OnInit(Entity<XenomorphReaperComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextGain = _timing.CurTime + TimeSpan.FromSeconds(1);
        Refresh(ent);
    }

    private void OnRemoved(Entity<XenomorphReaperComponent> ent, ref ComponentRemove args)
        => _alerts.ClearAlert(ent, "DumontXenomorphFlesh");

    private void Refresh(Entity<XenomorphReaperComponent> ent)
    {
        Dirty(ent);
        _alerts.ShowAlert(ent, "DumontXenomorphFlesh",
            (short)(18 - Math.Clamp(ent.Comp.Flesh * 18 / Math.Max(1, ent.Comp.MaxFlesh), 0, 18)));
    }

    private void OnAlert(Entity<XenomorphReaperComponent> ent, ref XenomorphFleshAlertEvent args)
    {
        _popup.PopupEntity(Loc.GetString("xenomorph-reaper-flesh", ("amount", ent.Comp.Flesh), ("max", ent.Comp.MaxFlesh)), ent, ent);
        args.Handled = true;
    }

    private void AddFlesh(Entity<XenomorphReaperComponent> ent, int amount)
    {
        ent.Comp.Flesh = Math.Clamp(ent.Comp.Flesh + amount, 0, ent.Comp.MaxFlesh);
        Refresh(ent);
    }

    private bool HasFlesh(Entity<XenomorphReaperComponent> ent, int amount)
    {
        if (ent.Comp.Flesh >= amount) return true;
        _popup.PopupEntity(Loc.GetString("xenomorph-reaper-no-flesh", ("amount", amount)), ent, ent);
        return false;
    }

    private bool Spend(Entity<XenomorphReaperComponent> ent, int amount)
    {
        if (!HasFlesh(ent, amount)) return false;
        AddFlesh(ent, -amount);
        return true;
    }

    private void Toggle<T>(EntityUid uid, bool active)
    {
        foreach (var action in _actions.GetActions(uid))
            if (_actions.GetEvent(action) is T) _actions.SetToggled(action.Owner, active);
    }

    private void OnMelee(Entity<XenomorphReaperComponent> ent, ref MeleeHitEvent args)
    {
        if (args.IsHit && args.HitEntities.Any(Enemy)) AddFlesh(ent, ent.Comp.MeleeGain);
    }

    private bool CanHarvest(EntityUid user, EntityUid target) => CanUse(user)
        && HasComp<HumanoidAppearanceComponent>(target) && !Friendly(target) && _mobs.IsDead(target)
        && IsIrrecoverable(target)
        && !HasComp<XenomorphFleshHarvestedComponent>(target)
        && _interaction.InRangeUnobstructed(user, target, 1.5f);

    private bool IsIrrecoverable(EntityUid target)
    {
        if (_rotting.IsRotten(target) || HasComp<UnrevivableComponent>(target))
            return true;
        return TryComp<MindContainerComponent>(target, out var container)
            && (!TryComp<MindComponent>(container.Mind, out var mind) || mind.UserId == null);
    }

    private void OnHarvest(Entity<XenomorphReaperComponent> ent, ref XenomorphFleshHarvestActionEvent args)
    {
        if (args.Handled) return;
        if (!CanHarvest(ent, args.Target))
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-reaper-harvest-invalid"), ent, ent);
            return;
        }
        if (!_plasma.HasPlasma(ent, 50)) return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, 4,
            new XenomorphFleshHarvestDoAfterEvent(), ent.Owner, args.Target)
        { BreakOnMove = true, NeedHand = false });
        if (!args.Handled) return;
        _plasma.ChangePlasmaAmount(ent, -50);
        Toggle<XenomorphFleshHarvestActionEvent>(ent, true);
    }

    private void OnHarvested(Entity<XenomorphReaperComponent> ent, ref XenomorphFleshHarvestDoAfterEvent args)
    {
        Toggle<XenomorphFleshHarvestActionEvent>(ent, false);
        if (args.Cancelled || args.Handled || args.Target is not { } target || !CanHarvest(ent, target)) return;
        EnsureComp<XenomorphFleshHarvestedComponent>(target);
        var limbs = new HashSet<EntityUid>();
        foreach (var type in new[] { BodyPartType.Arm, BodyPartType.Hand, BodyPartType.Leg, BodyPartType.Foot })
            foreach (var limb in _body.GetBodyChildrenOfType(target, type)) limbs.Add(limb.Id);
        var coordinates = _transform.GetMoverCoordinates(target);
        var index = 0;
        foreach (var limb in limbs)
        {
            if (!_containers.TryGetContainingContainer(limb, out var container) || !_containers.Remove(limb, container)) continue;
            _transform.SetCoordinates(limb, coordinates);
            var angle = MathF.Tau * index++ / limbs.Count;
            _throwing.TryThrow(limb, new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 1.5f, 4, user: ent);
        }
        AddFlesh(ent, 150);
        args.Handled = true;
    }

    private void OnRapture(Entity<XenomorphReaperComponent> ent, ref XenomorphRaptureActionEvent args)
    {
        if (args.Handled || !CanUse(ent) || !_blocker.CanAttack(ent) || !Enemy(args.Target)
            || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, 3)) return;
        _damage.TryChangeDamage(args.Target, new DamageSpecifier { DamageDict = { ["Blunt"] = 20, ["Cellular"] = 15 } }, origin: ent);
        AddFlesh(ent, 60);
        _audio.PlayPvs(new SoundCollectionSpecifier("AlienClaw"), ent);
        Spawn("DumontXenomorphRaptureEffect", new EntityCoordinates(args.Target, Vector2.Zero));
        var delta = _transform.GetWorldPosition(args.Target) - _transform.GetWorldPosition(ent);
        _melee.DoLunge(ent, ent, ent, Angle.Zero, (-_transform.GetWorldRotation(ent)).RotateVec(delta),
            "WeaponArcThrust", Angle.Zero, false, predicted: false);
        args.Handled = true;
    }

    private EntityCoordinates Snap(EntityCoordinates coordinates)
    {
        var grid = _transform.GetGrid(coordinates) ?? coordinates.EntityId;
        var local = _transform.ToCoordinates(grid, _transform.ToMapCoordinates(coordinates));
        return new EntityCoordinates(grid, MathF.Floor(local.X) + 0.5f, MathF.Floor(local.Y) + 0.5f);
    }

    private bool InRange(EntityUid uid, EntityCoordinates target) => target.IsValid(EntityManager)
        && _interaction.InRangeUnobstructed(uid, target, 7);

    private void OnBloom(Entity<XenomorphReaperComponent> ent, ref XenomorphFleshBloomActionEvent args)
    {
        if (args.Handled || !CanUse(ent) || !HasFlesh(ent, 100) || !InRange(ent, args.Target)) return;
        var center = Snap(args.Target);
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, ent, 2,
            new XenomorphFleshBloomDoAfterEvent { Coordinates = GetNetCoordinates(center) }, ent.Owner)
        { BreakOnMove = true, NeedHand = false });
        if (!args.Handled) return;
        Toggle<XenomorphFleshBloomActionEvent>(ent, true);
        _audio.PlayPvs(new SoundCollectionSpecifier("XenoResinBreak"), ent);
        for (var x = -1; x <= 1; x++) for (var y = -1; y <= 1; y++)
            Spawn("DumontXenomorphFleshBloomTelegraph", new EntityCoordinates(center.EntityId, center.X + x, center.Y + y));
    }

    private void OnBloomed(Entity<XenomorphReaperComponent> ent, ref XenomorphFleshBloomDoAfterEvent args)
    {
        Toggle<XenomorphFleshBloomActionEvent>(ent, false);
        if (args.Handled || args.Cancelled || !CanUse(ent)) return;
        var center = GetCoordinates(args.Coordinates);
        if (!InRange(ent, center) || !Spend(ent, 100)) return;
        for (var x = -1; x <= 1; x++) for (var y = -1; y <= 1; y++)
        {
            var tile = new EntityCoordinates(center.EntityId, center.X + x, center.Y + y);
            if (_interaction.InRangeUnobstructed(_transform.ToMapCoordinates(center), _transform.ToMapCoordinates(tile), 2)) SpawnField(ent, tile, true);
        }
        args.Handled = true;
    }

    private void OnGas(Entity<XenomorphReaperComponent> ent, ref XenomorphRedGasActionEvent args)
    {
        if (args.Handled || !CanUse(ent) || !HasFlesh(ent, 20) || !InRange(ent, args.Target)) return;
        var start = Snap(_transform.GetMoverCoordinates(ent));
        var target = _transform.ToCoordinates(start.EntityId, _transform.ToMapCoordinates(args.Target));
        var path = XenomorphReaperGeometry.GasPath(start.Position, target.Position, 7)
            .Select(point => GetNetCoordinates(new EntityCoordinates(start.EntityId, point))).ToArray();
        args.Handled = StartGasStep(ent, path, 0);
        if (args.Handled) Toggle<XenomorphRedGasActionEvent>(ent, true);
    }

    private bool StartGasStep(EntityUid uid, NetCoordinates[] path, int step) => _doAfter.TryStartDoAfter(
        new DoAfterArgs(EntityManager, uid, 0.2f, new XenomorphRedGasDoAfterEvent { Path = path, Step = step }, uid)
        { BreakOnMove = false, NeedHand = false });

    private void OnGasStep(Entity<XenomorphReaperComponent> ent, ref XenomorphRedGasDoAfterEvent args)
    {
        if (args.Handled) return;
        args.Handled = true;
        if (args.Cancelled || !CanUse(ent) || args.Step >= args.Path.Length)
        { Toggle<XenomorphRedGasActionEvent>(ent, false); return; }
        var center = GetCoordinates(args.Path[args.Step]);
        var path = args.Path.Select(point => GetCoordinates(point).Position).ToArray();
        foreach (var point in XenomorphReaperGeometry.GasTiles(path, args.Step))
        {
            var tile = new EntityCoordinates(center.EntityId, point);
            if (!_interaction.InRangeUnobstructed(_transform.ToMapCoordinates(GetCoordinates(args.Path[0])), _transform.ToMapCoordinates(tile), 9)) continue;
            if (!Spend(ent, 20)) { Toggle<XenomorphRedGasActionEvent>(ent, false); return; }
            SpawnField(ent, tile, false);
        }
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/smoke.ogg", AudioParams.Default.WithVolume(-8)), ent);
        if (args.Step + 1 >= args.Path.Length || !StartGasStep(ent, args.Path, args.Step + 1))
            Toggle<XenomorphRedGasActionEvent>(ent, false);
    }

    private void SpawnField(EntityUid reaper, EntityCoordinates tile, bool bloom)
    {
        var uid = Spawn(bloom ? "DumontXenomorphFleshBloom" : "DumontXenomorphRedGas", tile);
        Comp<XenomorphReaperFieldComponent>(uid).Reaper = reaper;
    }

    private void OnMantle(Entity<XenomorphReaperComponent> ent, ref XenomorphCarrionMantleActionEvent args)
    {
        if (args.Handled || !CanUse(ent) || !HasComp<XenomorphComponent>(args.Target) || _mobs.IsDead(args.Target)
            || !_interaction.InRangeUnobstructed(ent.Owner, args.Target, 5) || !Spend(ent, 100)) return;
        var mantle = EnsureComp<XenomorphCarrionMantleComponent>(args.Target);
        if (mantle.Effect is { } old) QueueDel(old);
        mantle.Active = true;
        mantle.Shield = 200;
        mantle.Caster = ent;
        mantle.ExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(8);
        mantle.Effect = Spawn("DumontXenomorphCarrionMantleEffect", new EntityCoordinates(args.Target, Vector2.Zero));
        UpdateShieldVisual(mantle);
        Dirty(args.Target, mantle);
        _movement.RefreshMovementSpeedModifiers(args.Target);
        _armor.RefreshAlert(args.Target);
        Toggle<XenomorphCarrionMantleActionEvent>(ent, true);
        args.Handled = true;
    }

    private void UpdateShieldVisual(XenomorphCarrionMantleComponent mantle)
    {
        if (mantle.Effect is { } effect && !TerminatingOrDeleted(effect))
            _appearance.SetData(effect, XenomorphReaperVisuals.ShieldFraction, mantle.Shield / 200f);
    }

    private void OnShieldDamage(Entity<XenomorphCarrionMantleComponent> ent, ref DamageModifyEvent args)
    {
        if (ent.Comp.Shield <= 0) return;
        foreach (var (type, value) in args.Damage.DamageDict)
        {
            if (value <= 0) continue;
            var absorbed = Math.Min(value.Float(), ent.Comp.Shield);
            ent.Comp.Shield -= absorbed;
            args.Damage.DamageDict[type] -= FixedPoint2.New(absorbed);
        }
        if (ent.Comp.Shield <= 0 && ent.Comp.Effect is { } effect)
        { QueueDel(effect); ent.Comp.Effect = null; }
        UpdateShieldVisual(ent.Comp);
        Dirty(ent);
    }

    private void OnMantleRemoved(Entity<XenomorphCarrionMantleComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.Effect is { } effect) QueueDel(effect);
        if (ent.Comp.Caster is { } caster && Exists(caster)) Toggle<XenomorphCarrionMantleActionEvent>(caster, false);
        ent.Comp.Active = false;
        _armor.RefreshAlert(ent);
    }

    public override void Update(float frameTime)
    {
        var now = _timing.CurTime;
        var reapers = EntityQueryEnumerator<XenomorphReaperComponent>();
        while (reapers.MoveNext(out var uid, out var reaper))
        {
            if (now < reaper.NextGain) continue;
            reaper.NextGain = now + TimeSpan.FromSeconds(1);
            if (_mobs.IsAlive(uid) && reaper.Flesh < reaper.PassiveCap)
                AddFlesh((uid, reaper), Math.Min(reaper.PassiveGain, reaper.PassiveCap - reaper.Flesh));
        }
        var fields = EntityQueryEnumerator<XenomorphReaperFieldComponent>();
        while (fields.MoveNext(out var uid, out var field))
        {
            if (!Exists(field.Reaper)) { QueueDel(uid); continue; }
            if (now < field.NextPulse) continue;
            field.NextPulse = now + TimeSpan.FromSeconds(1);
            foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(_transform.GetMoverCoordinates(uid), 0.5f))
            {
                if (_mobs.IsDead(target) || !_interaction.InRangeUnobstructed(uid, target.Owner, 0.75f)) continue;
                if (Friendly(target))
                {
                    if (field.Bloom) _damage.TryChangeDamage(target,
                        new DamageSpecifier { DamageDict = { ["Blunt"] = -8, ["Slash"] = -8 } }, ignoreResistances: true);
                    continue;
                }
                if (!Enemy(target)) continue;
                _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Poison"] = field.Bloom ? 8 : 5 }, ArmorPenetration = 0.1f }, origin: field.Reaper);
                if (field.Bloom && TryComp<MovementSpeedModifierComponent>(target, out var speed))
                    _slow.TryUpdateMovementSpeedModDuration(target, "XenomorphStompSlowdown", TimeSpan.FromSeconds(1.5),
                        1 / (1 + Math.Max(speed.CurrentSprintSpeed, speed.BaseSprintSpeed) * 0.2f));
            }
        }
        var mantles = EntityQueryEnumerator<XenomorphCarrionMantleComponent>();
        while (mantles.MoveNext(out var uid, out var mantle))
        {
            if (now < mantle.ExpiresAt) continue;
            if (mantle.Active)
            {
                mantle.Active = false;
                if (mantle.Caster is { } caster && Exists(caster)) Toggle<XenomorphCarrionMantleActionEvent>(caster, false);
                _movement.RefreshMovementSpeedModifiers(uid);
                _armor.RefreshAlert(uid);
            }
            mantle.Shield = Math.Max(0, mantle.Shield - mantle.ShieldDecay * frameTime);
            UpdateShieldVisual(mantle);
            Dirty(uid, mantle);
            if (mantle.Shield <= 0) RemCompDeferred<XenomorphCarrionMantleComponent>(uid);
        }
    }
}
