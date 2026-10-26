using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.FaceHugger;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.DoAfter;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Egg;
using Content.Shared._White.Xenomorphs.Infection;
using Content.Shared._White.Xenomorphs.Larva;
using Content.Shared.Clothing.Components;
using Robust.Shared.Containers;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.Physics;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphResinHoleComponent : Component
{
    [DataField] public bool Filled;
    [DataField] public bool Acid;
}

public sealed partial class XenomorphCarrierConstructionSystem : EntitySystem
{
    [Dependency] private XenomorphAcidTrapSystem _acidTrap = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private DoAfterSystem _doAfter = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphPortableEggComponent, AfterInteractEvent>(OnPlantEgg);
        SubscribeLocalEvent<XenomorphPortableEggComponent, XenomorphPlantEggDoAfterEvent>(OnEggPlanted);
        SubscribeLocalEvent<XenomorphComponent, XenomorphResinHoleActionEvent>(OnBuildHole);
        SubscribeLocalEvent<XenomorphResinHoleComponent, InteractUsingEvent>(OnFillHole);
        SubscribeLocalEvent<XenomorphResinHoleComponent, XenomorphFillHoleDoAfterEvent>(OnHoleFilled);
        SubscribeLocalEvent<XenomorphResinHoleComponent, StepTriggerAttemptEvent>(OnTriggerAttempt);
        SubscribeLocalEvent<XenomorphResinHoleComponent, StepTriggeredOffEvent>(OnTriggered);
        SubscribeLocalEvent<XenomorphResinHoleComponent, DamageChangedEvent>(OnDamaged);
    }

    private EntityCoordinates Snap(EntityCoordinates coordinates)
    {
        var xform = Transform(coordinates.EntityId);
        var grid = xform.GridUid ?? coordinates.EntityId;
        var local = _transform.ToCoordinates(grid, _transform.ToMapCoordinates(coordinates));
        return new EntityCoordinates(grid, MathF.Floor(local.X) + 0.5f, MathF.Floor(local.Y) + 0.5f);
    }

    private bool CanPlant(EntityUid user, EntityCoordinates coordinates, bool hole)
    {
        if (!_mobs.IsAlive(user) || !HasComp<XenomorphComponent>(user)
            || !_blocker.CanInteract(user, null)
            || !_interaction.InRangeUnobstructed(user, coordinates,
                range: TryComp<XenomorphCarrierComponent>(user, out var carrier) && carrier.CanGenerateEggs ? 2.5f : 1.5f))
            return false;
        var tile = _turf.GetTileRef(coordinates);
        if (tile is not { } turf || turf.Tile.IsEmpty || _turf.IsTileBlocked(turf, CollisionGroup.Impassable))
            return false;
        var weeds = false;
        foreach (var item in _lookup.GetEntitiesIntersecting(coordinates))
        {
            if (HasComp<XenomorphWeedWallSpreadComponent>(item))
                weeds = true;
            if (HasComp<XenomorphEggComponent>(item) || HasComp<XenomorphResinHoleComponent>(item))
                return false;
        }
        if (hole)
            foreach (var nearby in _lookup.GetEntitiesInRange<XenomorphResinHoleComponent>(coordinates, 1.5f))
                return false;
        return weeds || !hole && carrier is { CanGenerateEggs: true };
    }

    private bool HasWeeds(EntityCoordinates coordinates)
    {
        foreach (var item in _lookup.GetEntitiesIntersecting(coordinates))
            if (HasComp<XenomorphWeedWallSpreadComponent>(item))
                return true;
        return false;
    }

    private void OnPlantEgg(Entity<XenomorphPortableEggComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !_hands.IsHolding(args.User, ent))
            return;
        var coordinates = Snap(args.ClickLocation);
        if (!CanPlant(args.User, coordinates, false) || !_plasma.HasPlasma(args.User, 30))
            return;
        var ev = new XenomorphPlantEggDoAfterEvent { Coordinates = GetNetCoordinates(coordinates) };
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User,
            HasComp<XenomorphCarrierComponent>(args.User) ? 1 : 3, ev, ent.Owner, used: ent.Owner)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            NeedHand = true,
        });
    }

    private void OnEggPlanted(Entity<XenomorphPortableEggComponent> ent, ref XenomorphPlantEggDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || EntityManager.IsQueuedForDeletion(ent) || !_hands.IsHolding(args.User, ent))
            return;
        var coordinates = GetCoordinates(args.Coordinates);
        if (!CanPlant(args.User, coordinates, false) || !_plasma.HasPlasma(args.User, 30))
            return;
        _plasma.ChangePlasmaAmount(args.User, -30);
        var planted = Spawn("DumontXenomorphEgg", coordinates);
        if (!HasWeeds(coordinates))
            Sustain(planted, args.User);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg"), planted);
        QueueDel(ent);
        args.Handled = true;
    }

    private void OnBuildHole(Entity<XenomorphComponent> ent, ref XenomorphResinHoleActionEvent args)
    {
        if (args.Handled || !(HasComp<XenomorphCarrierComponent>(ent) || HasComp<XenomorphBurrowComponent>(ent))
            || !_plasma.HasPlasma(ent, 200))
            return;
        var coordinates = Snap(_transform.GetMoverCoordinates(ent));
        if (!CanPlant(ent, coordinates, true))
            return;
        Spawn("DumontXenomorphResinHole", coordinates);
        _plasma.ChangePlasmaAmount(ent, -200);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_resin_build1.ogg"), ent);
        args.Handled = true;
    }

    private bool CanFill(EntityUid user, EntityUid parasite) => HasComp<XenomorphComponent>(user)
        && _mobs.IsAlive(user) && _blocker.CanInteract(user, parasite)
        && _hands.IsHolding(user, parasite)
        && (!TryComp<ClothingComponent>(parasite, out var clothing) || clothing.InSlot == null)
        && HasComp<FaceHuggerComponent>(parasite) && !_mobs.IsDead(parasite)
        && !EntityManager.IsQueuedForDeletion(parasite)
        && (!TryComp<MindContainerComponent>(parasite, out var mind) || !mind.HasMind);

    private void OnFillHole(Entity<XenomorphResinHoleComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || ent.Comp.Filled || !CanFill(args.User, args.Used))
            return;
        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, 3,
            new XenomorphFillHoleDoAfterEvent(), ent.Owner, ent.Owner, args.Used)
        {
            BreakOnMove = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            NeedHand = true,
        });
        if (args.Handled)
        {
            var until = _timing.CurTime + TimeSpan.FromSeconds(3.5);
            EnsureComp<XenomorphCarrierRecoveryComponent>(args.Used).RecoverAt = until;
            var facehugger = Comp<FaceHuggerComponent>(args.Used);
            facehugger.Active = false;
            facehugger.RestIn = until;
        }
    }

    private void OnHoleFilled(Entity<XenomorphResinHoleComponent> ent, ref XenomorphFillHoleDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || ent.Comp.Filled || args.Used is not { } used || !CanFill(args.User, used))
            return;
        var container = _containers.EnsureContainer<ContainerSlot>(ent, "parasite");
        if (!_containers.Insert(used, container))
            return;
        ent.Comp.Filled = true;
        _appearance.SetData(ent, XenomorphCarrierVisuals.TrapFilled, true);
        _appearance.SetData(ent, XenomorphCarrierVisuals.TrapContent, XenomorphTrapContent.Parasite);
        args.Handled = true;
    }

    private bool CanTrigger(EntityUid hole, EntityUid target)
    {
        if (HasComp<XenomorphComponent>(target) || HasComp<FaceHuggerComponent>(target) || !_mobs.IsAlive(target))
            return false;
        if (Comp<XenomorphResinHoleComponent>(hole).Acid)
            return (!TryComp<Content.Shared.Standing.StandingStateComponent>(target, out var standing) || standing.Standing)
                && _interaction.InRangeUnobstructed(hole, target, 1.5f);
        return HasComp<InventoryComponent>(target)
            && !HasComp<XenomorphInfectedComponent>(target) && !HasComp<XenomorphLarvaVictimComponent>(target)
            && (!_inventory.TryGetSlotEntity(target, "mask", out var mask) || !HasComp<FaceHuggerComponent>(mask))
            && _interaction.InRangeUnobstructed(hole, target, 0.5f);
    }

    private void OnTriggerAttempt(Entity<XenomorphResinHoleComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = ent.Comp.Filled && CanTrigger(ent, args.Tripper);
    }

    private void OnTriggered(Entity<XenomorphResinHoleComponent> ent, ref StepTriggeredOffEvent args)
    {
        if (!ent.Comp.Filled || !CanTrigger(ent, args.Tripper))
            return;
        if (!ent.Comp.Acid)
            _stun.TryUpdateParalyzeDuration(args.Tripper, TimeSpan.FromSeconds(2));
        Release(ent);
    }

    private void OnDamaged(Entity<XenomorphResinHoleComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageIncreased && ent.Comp.Filled)
            Release(ent);
    }

    private void Sustain(EntityUid egg, EntityUid owner)
    {
        var comp = Comp<XenomorphEggSupportComponent>(egg);
        comp.Fragile = comp.Sustained = true;
        comp.Sustainer = owner;
        comp.ExpiresAt = _timing.CurTime + TimeSpan.FromMinutes(5);
        comp.ShortExpiresAt = _timing.CurTime + TimeSpan.FromMinutes(1);
        Dirty(egg, comp);
        var count = 0;
        Entity<XenomorphEggSupportComponent>? oldest = null;
        var query = EntityQueryEnumerator<XenomorphEggSupportComponent>();
        while (query.MoveNext(out var uid, out var other))
        {
            if (other.Sustainer != owner || other.DecayAt != null)
                continue;
            count++;
            if (oldest == null || other.ExpiresAt < oldest.Value.Comp.ExpiresAt)
                oldest = (uid, other);
        }
        if (count > 4 && oldest is { } excess)
            StartDecay(excess);
    }

    private void StartDecay(Entity<XenomorphEggSupportComponent> ent)
    {
        if (ent.Comp.DecayAt != null)
            return;
        ent.Comp.DecayAt = _timing.CurTime + TimeSpan.FromSeconds(10);
        ent.Comp.Sustainer = null;
    }

    public void DecaySustainedEggs(EntityUid owner)
    {
        var query = EntityQueryEnumerator<XenomorphEggSupportComponent>();
        while (query.MoveNext(out var uid, out var support))
            if (support.Sustainer == owner)
                StartDecay((uid, support));
    }

    public override void Update(float frameTime)
    {
        var now = _timing.CurTime;
        var holes = EntityQueryEnumerator<XenomorphResinHoleComponent>();
        while (holes.MoveNext(out var uid, out var hole))
        {
            if (!hole.Filled)
                continue;
            foreach (var target in _lookup.GetEntitiesInRange<Content.Shared.Mobs.Components.MobStateComponent>(_transform.GetMoverCoordinates(uid), hole.Acid ? 1.5f : 0.5f))
            {
                if (!CanTrigger(uid, target))
                    continue;
                if (!hole.Acid)
                    _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(2));
                Release((uid, hole));
                break;
            }
        }
        var eggs = EntityQueryEnumerator<XenomorphEggSupportComponent, XenomorphEggComponent>();
        while (eggs.MoveNext(out var uid, out var support, out var egg))
        {
            if (egg.Status == XenomorphEggStatus.Burst)
                continue;
            if (support.DecayAt != null)
            {
                if (now >= support.DecayAt)
                {
                    Spawn("DumontXenomorphEggDecayEffect", _transform.GetMoverCoordinates(uid));
                    _audio.PlayPvs(new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_egg_burst.ogg"), uid);
                    QueueDel(uid);
                }
                continue;
            }
            if (now < support.NextCheck)
                continue;
            support.NextCheck = now + TimeSpan.FromSeconds(2);
            if (HasWeeds(_transform.GetMoverCoordinates(uid)))
            {
                if (support.Fragile)
                {
                    support.Fragile = support.Sustained = false;
                    support.Sustainer = null;
                    Dirty(uid, support);
                }
                continue;
            }
            if (!support.Fragile)
            {
                support.Fragile = true;
                support.ExpiresAt = now + TimeSpan.FromMinutes(6);
                Dirty(uid, support);
            }
            if (support.Sustainer is { } owner && Exists(owner))
            {
                var ownerPosition = _transform.GetMapCoordinates(owner);
                var position = _transform.GetMapCoordinates(uid);
                if (position.MapId == ownerPosition.MapId
                    && Vector2.DistanceSquared(position.Position, ownerPosition.Position) <= 14 * 14)
                    support.ShortExpiresAt = now + TimeSpan.FromMinutes(1);
                else if (now >= support.ShortExpiresAt)
                    StartDecay((uid, support));
            }
            if (now >= support.ExpiresAt)
                StartDecay((uid, support));
        }
    }

    private void Release(Entity<XenomorphResinHoleComponent> ent)
    {
        if (!ent.Comp.Filled)
            return;
        ent.Comp.Filled = false;
        _appearance.SetData(ent, XenomorphCarrierVisuals.TrapFilled, false);
        _appearance.SetData(ent, XenomorphCarrierVisuals.TrapContent, XenomorphTrapContent.Empty);
        if (ent.Comp.Acid)
        {
            ent.Comp.Acid = false;
            _acidTrap.Burst(ent);
            return;
        }
        if (!_containers.TryGetContainer(ent, "parasite", out var container)
            || container is not ContainerSlot { ContainedEntity: { } parasite }
            || !_containers.Remove(parasite, container))
            return;
        _transform.SetCoordinates(parasite, _transform.GetMoverCoordinates(ent));
        RemComp<XenomorphCarrierRecoveryComponent>(parasite);
        var facehugger = Comp<FaceHuggerComponent>(parasite);
        facehugger.Active = true;
        facehugger.RestIn = _timing.CurTime;
    }
}
