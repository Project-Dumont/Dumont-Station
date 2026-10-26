using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.DoAfter;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._White.Xenomorphs.Egg;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Buckle.Components;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphFruitSystem : EntitySystem
{
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private TagSystem _tags = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphFruitPlanterComponent, XenomorphPlantFruitActionEvent>(OnPlant);
        SubscribeLocalEvent<XenomorphFruitPlanterComponent, XenomorphChooseFruitActionEvent>(OnChoose);
        Subs.BuiEvents<XenomorphFruitPlanterComponent>(XenomorphFruitUiKey.Choose, subs => subs.Event<XenomorphChooseFruitMessage>(OnChosen));
        SubscribeLocalEvent<XenomorphFruitBuffComponent, Content.Shared.Weapons.Melee.Events.MeleeHitEvent>(OnBuffMelee);
        SubscribeLocalEvent<XenomorphFruitBuffComponent, DamageModifyEvent>(OnBuffDamage);
        SubscribeLocalEvent<XenomorphFruitBuffComponent, ComponentRemove>(OnBuffRemoved);
        SubscribeLocalEvent<XenomorphFruitComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<XenomorphFruitComponent, EntityTerminatingEvent>(OnFruitRemoved);
        SubscribeLocalEvent<XenomorphFruitComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<XenomorphFruitComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<XenomorphFruitComponent, GetVerbsEvent<ActivationVerb>>(OnHarvestVerb);
        SubscribeLocalEvent<XenomorphFruitComponent, GetVerbsEvent<AlternativeVerb>>(OnConsumeVerb);
        SubscribeLocalEvent<XenomorphFruitComponent, XenomorphFruitHarvestDoAfterEvent>(OnHarvested);
        SubscribeLocalEvent<XenomorphFruitComponent, XenomorphFruitConsumeDoAfterEvent>(OnConsumed);
        SubscribeLocalEvent<XenomorphFruitComponent, ExaminedEvent>(OnExamined);
    }

    private void SetState(Entity<XenomorphFruitComponent> ent, XenomorphFruitState state)
    {
        ent.Comp.State = state;
        Dirty(ent);
        _appearance.SetData(ent, XenomorphFruitVisuals.State, state);
    }

    private void OnFruitRemoved(Entity<XenomorphFruitComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.Planter is not { } planter || !TryComp<XenomorphFruitPlanterComponent>(planter, out var comp)
            || !comp.Fruit.Remove(ent)) return;
        UpdateFruitUi((planter, comp));
    }

    private void OnInit(Entity<XenomorphFruitComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.GrowAt = _timing.CurTime + ent.Comp.GrowTime;
        SetState(ent, ent.Comp.State);
    }

    private void OnPlant(Entity<XenomorphFruitPlanterComponent> ent, ref XenomorphPlantFruitActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanConsciouslyPerformAction(ent)
            || TryComp<XenomorphRestComponent>(ent, out var rest) && rest.Resting) return;
        if (ent.Comp.Choice is not { } choice || !ent.Comp.Allowed.Contains(choice))
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-fruit-select-first"), ent, ent);
            return;
        }
        var coordinates = _transform.GetMoverCoordinates(ent);
        var grid = Transform(ent).GridUid;
        if (grid is not { } gridUid) return;
        var local = _transform.ToCoordinates(gridUid, _transform.ToMapCoordinates(coordinates));
        coordinates = new EntityCoordinates(gridUid, MathF.Floor(local.X) + 0.5f, MathF.Floor(local.Y) + 0.5f);
        var tile = _turf.GetTileRef(coordinates);
        var weeds = false;
        var blocked = tile is not { } floor || floor.Tile.IsEmpty || _turf.IsTileBlocked(floor,
            CollisionGroup.Impassable | CollisionGroup.MidImpassable | CollisionGroup.HighImpassable);
        foreach (var uid in _lookup.GetEntitiesIntersecting(coordinates))
        {
            if (HasComp<XenomorphWeedWallSpreadComponent>(uid)) weeds = true;
            if (uid == ent.Owner || !Transform(uid).Anchored) continue;
            if (HasComp<XenomorphFruitComponent>(uid) || HasComp<XenomorphResinHoleComponent>(uid)
                || HasComp<XenomorphEggComponent>(uid) || HasComp<StrapComponent>(uid)
                || HasComp<Content.Server._White.Spawners.Components.AreaSpawnerComponent>(uid)
                || _tags.HasTag(uid, "Structure") || _tags.HasTag(uid, "Airlock")) blocked = true;
        }
        if (blocked || !weeds)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-fruit-plant-blocked"), ent, ent);
            return;
        }
        if (!_plasma.ChangePlasmaAmount(ent, -ent.Comp.PlasmaCost)) return;
        _damage.TryChangeDamage(ent, new DamageSpecifier { DamageDict = { ["Blunt"] = 50 } }, ignoreResistances: true,
            targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAll);
        var fruit = Spawn("DumontXenomorphFruit" + choice, coordinates);
        var planted = Comp<XenomorphFruitComponent>(fruit);
        planted.Planter = ent;
        foreach (var uid in _lookup.GetEntitiesIntersecting(coordinates))
            if (HasComp<XenomorphHardyWeedComponent>(uid))
            {
                planted.GrowTime *= 0.8;
                planted.GrowAt = _timing.CurTime + planted.GrowTime;
                break;
            }
        ent.Comp.Fruit.RemoveAll(uid => !Exists(uid) || EntityManager.IsQueuedForDeletion(uid));
        ent.Comp.Fruit.Add(fruit);
        if (ent.Comp.Fruit.Count > ent.Comp.Maximum)
        {
            QueueDel(ent.Comp.Fruit[0]);
            ent.Comp.Fruit.RemoveAt(0);
            _popup.PopupEntity(Loc.GetString("xenomorph-fruit-overflow"), ent, ent);
        }
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_resin_build1.ogg", AudioParams.Default.WithVolume(-10)), ent);
        UpdateFruitUi(ent);
        args.Handled = true;
    }

    private bool CanUse(EntityUid user, EntityUid fruit)
        => HasComp<HandsComponent>(user) && _blocker.CanInteract(user, fruit)
            && _interaction.InRangeUnobstructed(user, fruit, 1.5f);

    private void Harvest(Entity<XenomorphFruitComponent> ent, EntityUid user)
    {
        if (ent.Comp.State is XenomorphFruitState.Item or XenomorphFruitState.Eaten || !CanUse(user, ent)) return;
        if (ent.Comp.State == XenomorphFruitState.Growing && HasComp<XenomorphComponent>(user)
            && !HasComp<XenomorphFruitPlanterComponent>(user)) return;
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(2 * (TryComp<XenomorphFruitPlanterComponent>(user, out var planter) ? planter.HarvestMultiplier : 1)), new XenomorphFruitHarvestDoAfterEvent(), ent.Owner, ent.Owner)
        { NeedHand = true, BreakOnMove = true, BlockDuplicate = true, DuplicateCondition = DuplicateConditions.SameEvent });
    }

    private void OnActivate(Entity<XenomorphFruitComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled) return;
        if (ent.Comp.State == XenomorphFruitState.Item) Consume(ent, args.User, args.User);
        else Harvest(ent, args.User);
        args.Handled = true;
    }

    private void OnHarvestVerb(Entity<XenomorphFruitComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || ent.Comp.State is XenomorphFruitState.Item or XenomorphFruitState.Eaten) return;
        var user = args.User;
        args.Verbs.Add(new ActivationVerb { Text = Loc.GetString("xenomorph-fruit-harvest"), Act = () => Harvest(ent, user) });
    }

    private void OnConsumeVerb(Entity<XenomorphFruitComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !HasComp<XenomorphComponent>(args.User)) return;
        var user = args.User;
        args.Verbs.Add(new AlternativeVerb { Text = Loc.GetString("xenomorph-fruit-consume"), Act = () => Consume(ent, user, user) });
    }

    private void OnHarvested(Entity<XenomorphFruitComponent> ent, ref XenomorphFruitHarvestDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !CanUse(args.User, ent)
            || ent.Comp.State is XenomorphFruitState.Item or XenomorphFruitState.Eaten) return;
        args.Handled = true;
        _audio.PlayPvs(new SoundCollectionSpecifier("XenoResinBreak", AudioParams.Default.WithVolume(-10)), ent);
        if (ent.Comp.State == XenomorphFruitState.Growing) { QueueDel(ent); return; }
        _transform.Unanchor(ent);
        SetState(ent, XenomorphFruitState.Item);
        _hands.TryPickup(args.User, ent);
    }

    private bool CanConsume(Entity<XenomorphFruitComponent> ent, EntityUid user, EntityUid target)
        => ent.Comp.State is XenomorphFruitState.Grown or XenomorphFruitState.Item
            && CanUse(user, ent) && HasComp<XenomorphComponent>(user) && HasComp<XenomorphComponent>(target)
            && !_mobs.IsDead(target) && _interaction.InRangeUnobstructed(user, target, 1.5f)
            && TryComp<DamageableComponent>(target, out var damage)
            && (ent.Comp.Kind is not (XenomorphFruitKind.Lesser or XenomorphFruitKind.Greater) || damage.TotalDamage > 0)
            && (ent.Comp.Kind != XenomorphFruitKind.Speed || !TryComp<XenomorphFruitBuffComponent>(target, out var buff) || buff.SpeedBonus <= 0);

    private void Consume(Entity<XenomorphFruitComponent> ent, EntityUid user, EntityUid target)
    {
        if (!CanConsume(ent, user, target)) return;
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(2 * (user != target && TryComp<XenomorphFruitPlanterComponent>(user, out var planter) ? planter.FeedingMultiplier : 1)), new XenomorphFruitConsumeDoAfterEvent(), ent.Owner, target, ent.Owner)
        { NeedHand = true, BreakOnMove = true, BreakOnHandChange = user != target,
            BlockDuplicate = true, DuplicateCondition = DuplicateConditions.SameEvent });
    }

    private void OnAfterInteract(Entity<XenomorphFruitComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is not { } target || !_hands.IsHolding(args.User, ent)) return;
        Consume(ent, args.User, target);
        args.Handled = true;
    }

    private void OnConsumed(Entity<XenomorphFruitComponent> ent, ref XenomorphFruitConsumeDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is not { } target || EntityManager.IsQueuedForDeletion(ent)
            || !CanConsume(ent, args.User, target)) return;
        ApplyFruit(ent.Comp.Kind, target);
        SetState(ent, XenomorphFruitState.Eaten);
        EnsureComp<TimedDespawnComponent>(ent).Lifetime = 1;
        Spawn("DumontXenomorphSalveHealEffect", new EntityCoordinates(target, Vector2.Zero));
        _popup.PopupEntity(Loc.GetString("xenomorph-fruit-healed"), target, target);
        args.Handled = true;
    }

    private void OnExamined(Entity<XenomorphFruitComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(ent.Comp.State == XenomorphFruitState.Growing ? "xenomorph-fruit-growing" : "xenomorph-fruit-ripe"));
        if (HasComp<XenomorphComponent>(args.Examiner)) args.PushMarkup(Loc.GetString("xenomorph-fruit-effect-" + ent.Comp.Kind.ToString().ToLowerInvariant()));
    }

    public override void Update(float frameTime)
    {
        UpdateBuffs(frameTime);
        var query = EntityQueryEnumerator<XenomorphFruitComponent>();
        while (query.MoveNext(out var uid, out var fruit))
            if (fruit.State == XenomorphFruitState.Growing && _timing.CurTime >= fruit.GrowAt)
                SetState((uid, fruit), XenomorphFruitState.Grown);
    }
    private void HealFruit(EntityUid target, FixedPoint2 budget)
    {
        var damage = Comp<DamageableComponent>(target);
        var remaining = budget;
        var healing = new DamageSpecifier();
        foreach (var type in new[] { "Blunt", "Slash", "Piercing", "Heat", "Shock", "Cold", "Caustic" })
        {
            if (remaining <= 0 || !damage.Damage.DamageDict.TryGetValue(type, out var value) || value <= 0) continue;
            var amount = FixedPoint2.Min(value, remaining);
            healing.DamageDict[type] = -amount;
            remaining -= amount;
        }
        _damage.TryChangeDamage(target, healing, ignoreResistances: true, targetPart: TargetBodyPart.All,
            splitDamage: SplitDamageBehavior.SplitEnsureAll);
    }

}
