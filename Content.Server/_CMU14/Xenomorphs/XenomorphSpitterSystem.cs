using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.Atmos.EntitySystems;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphSpitterSystem : EntitySystem
{
    [Dependency] private XenomorphPraetorianSystem _praetorian = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private XenomorphArmorSystem _armor = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private FlammableSystem _fire = default!;
    [Dependency] private AtmosphereSystem _atmos = default!;
    private readonly List<Spray> _sprays = new();
    private readonly HashSet<Entity<MobStateComponent>> _nearby = new();

    private sealed class Spray
    {
        public EntityUid User;
        public bool Strong;
        public bool Stacking;
        public float Range;
        public EntityCoordinates Start;
        public readonly List<(EntityCoordinates Coordinates, TimeSpan At)> Tiles = new();
        public readonly List<(EntityUid Effect, TimeSpan Until)> Pools = new();
        public readonly List<(EntityUid Target, TimeSpan At)> Barricades = new();
        public readonly HashSet<EntityUid> Hit = new();
        public readonly HashSet<(EntityUid Pool, EntityUid Victim)> Contacts = new();
    }

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphSpitterComponent, XenomorphChargeSpitActionEvent>(OnCharge);
        SubscribeLocalEvent<XenomorphSpitterComponent, XenomorphSprayAcidActionEvent>(OnSpray);
        SubscribeLocalEvent<XenomorphAcidedComponent, ComponentShutdown>(OnAcidRemoved);
        SubscribeLocalEvent<XenomorphAcidedComponent, ExtinguishEvent>(OnWash);
        SubscribeLocalEvent<XenomorphAcidedComponent, GetVerbsEvent<Verb>>(OnVerbs);
    }

    private void OnCharge(Entity<XenomorphSpitterComponent> ent, ref XenomorphChargeSpitActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanAttack(ent) || !_plasma.HasPlasma(ent, 50))
            return;
        ent.Comp.Charging = true;
        ent.Comp.Fired = false;
        ent.Comp.ExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(4);
        Dirty(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
        _armor.RefreshAlert(ent);
        _actions.SetToggled(args.Action.Owner, true);
        Spawn("DumontXenomorphEmpowerGreen", new EntityCoordinates(ent, Vector2.Zero));
        args.Handled = true;
    }

    public void InjectAcid(EntityUid target, int amount = 2)
    {
        if (HasComp<SiliconComponent>(target) || !_solutions.TryGetInjectableSolution(target, out var solution, out var contents)
            || solution == null || contents == null)
            return;
        if (contents.AvailableVolume < amount)
            _solutions.SplitSolution(solution.Value, amount - contents.AvailableVolume);
        _solutions.TryAddReagent(solution.Value, "DumontXenomorphMolecularAcid", amount, out _);
    }

    public void ShowAcidSplash(EntityUid target)
    {
        if (HasComp<XenomorphAcidedComponent>(target)) return;
        var children = Transform(target).ChildEnumerator;
        while (children.MoveNext(out var child))
            if (MetaData(child).EntityPrototype?.ID == "DumontXenomorphAcidSplash") return;
        Spawn("DumontXenomorphAcidSplash", new EntityCoordinates(target, Vector2.Zero));
    }

    public void ApplyAcid(EntityUid target)
    {
        if (HasComp<XenomorphComponent>(target) || HasComp<SiliconComponent>(target) || _mobs.IsDead(target))
            return;
        if (HasComp<XenomorphAcidedComponent>(target))
            return;
        var acid = AddComp<XenomorphAcidedComponent>(target);
        acid.ExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(20);
        acid.NextDamageAt = _timing.CurTime;
        acid.Effect = Spawn("DumontXenomorphAcidCoating", new EntityCoordinates(target, Vector2.Zero));
    }

    private void OnAcidRemoved(Entity<XenomorphAcidedComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Effect is { } effect && Exists(effect))
            QueueDel(effect);
    }

    private void OnWash(Entity<XenomorphAcidedComponent> ent, ref ExtinguishEvent args)
    {
        if (_timing.CurTime < ent.Comp.NextWashAt)
            return;
        ent.Comp.NextWashAt = _timing.CurTime + TimeSpan.FromSeconds(1);
        if (--ent.Comp.Resists <= 0)
            RemCompDeferred<XenomorphAcidedComponent>(ent);
    }

    private void OnVerbs(Entity<XenomorphAcidedComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (args.User != ent.Owner || !args.CanInteract)
            return;
        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("xenomorph-remove-acid"),
            Act = () =>
            {
                if (!_blocker.CanInteract(ent, null) || !HasComp<XenomorphAcidedComponent>(ent))
                    return;
                _stun.TryUpdateParalyzeDuration(ent, TimeSpan.FromSeconds(3));
                if (--ent.Comp.Resists <= 0)
                    RemCompDeferred<XenomorphAcidedComponent>(ent);
            },
        });
    }

    private void OnSpray(Entity<XenomorphSpitterComponent> ent, ref XenomorphSprayAcidActionEvent args)
    {
        if (TryComp<XenomorphBoilerComponent>(ent, out var boiler) && boiler.Preparing) return;
        if (args.Handled || !_mobs.IsAlive(ent) || !_blocker.CanAttack(ent) || !_plasma.HasPlasma(ent, ent.Comp.SprayCost)
            || Transform(ent).GridUid is not { } grid)
            return;
        args.Handled = CreateSpray(ent, args.Target, ent.Comp.SprayRange, ent.Comp.StrongSpray, ent.Comp.StackingSpray);
    }

    public bool CreateSpray(EntityUid user, EntityCoordinates targetCoordinates, float range, bool strong = false, bool stacking = false, double spreadDelay = .2)
    {
        if (Transform(user).GridUid is not { } grid) return false;
        var target = _transform.ToMapCoordinates(targetCoordinates);
        if (target.MapId != _transform.GetMapCoordinates(user).MapId)
            return false;
        var from = _transform.ToCoordinates(grid, _transform.GetMapCoordinates(user)).Position;
        var to = _transform.ToCoordinates(grid, target).Position;
        from = new Vector2(MathF.Floor(from.X) + 0.5f, MathF.Floor(from.Y) + 0.5f);
        to = new Vector2(MathF.Floor(to.X) + 0.5f, MathF.Floor(to.Y) + 0.5f);
        var delta = to - from;
        var length = delta.Length();
        if (!float.IsFinite(length) || length < 0.1f)
            return false;
        var direction = delta / length;
        var spray = new Spray { User = user, Start = new EntityCoordinates(grid, from), Strong = strong, Stacking = stacking, Range = range };
        var previous = from;
        var barricades = new HashSet<Entity<XenomorphSprayAcidableComponent>>();
        for (var i = 1; i <= MathF.Ceiling(Math.Min(length, spray.Range)); i++)
        {
            var point = from + direction * i;
            point = new Vector2(MathF.Floor(point.X) + 0.5f, MathF.Floor(point.Y) + 0.5f);
            if (point == previous)
                continue;
            previous = point;
            var coordinates = new EntityCoordinates(grid, point);
            if (!_interaction.InRangeUnobstructed(user, coordinates, range: spray.Range + 1))
            {
                _lookup.GetEntitiesInRange(_transform.ToMapCoordinates(coordinates), 0.4f, barricades);
                foreach (var barricade in barricades)
                    if (_interaction.InRangeUnobstructed(user, barricade.Owner, range: spray.Range + 1))
                        spray.Barricades.Add((barricade, _timing.CurTime + TimeSpan.FromSeconds(Math.Max(0, Vector2.Distance(point, from) - 1) * spreadDelay)));
                break;
            }
            spray.Tiles.Add((coordinates, _timing.CurTime + TimeSpan.FromSeconds(Math.Max(0, Vector2.Distance(point, from) - 1) * spreadDelay)));
        }
        if (spray.Tiles.Count == 0 && spray.Barricades.Count == 0)
            return false;
        _sprays.Add(spray);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/refill.ogg"), user);
        return true;
    }

    private void SprayHit(Spray spray, EntityUid pool, EntityUid target)
    {
        if (!spray.Contacts.Add((pool, target)) || target == spray.User)
            return;
        _fire.Extinguish(target);
        if (HasComp<XenomorphComponent>(target) || HasComp<SiliconComponent>(target) || _mobs.IsDead(target))
            return;
        _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Caustic"] = spray.Hit.Add(target) ? spray.Strong ? 36 : spray.Stacking ? 24 : 30 : 10 } }, origin: spray.User);
        if (spray.Stacking) _praetorian.ApplyStacks(target, 2, spray.User);
        ShowAcidSplash(target);
        if (spray.Strong) _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(2));
        EnhanceAcid(target);
    }

    public void EnhanceAcid(EntityUid target)
    {
        if (!TryComp<XenomorphAcidedComponent>(target, out var acid) || acid.Combo)
            return;
        acid.Combo = true;
        acid.ExpiresAt += TimeSpan.FromSeconds(40) - acid.Duration;
        acid.Duration = TimeSpan.FromSeconds(40);
        acid.Resists = 2;
        _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(2));
        if (acid.Effect is { } old)
            QueueDel(old);
        acid.Effect = Spawn("DumontXenomorphAcidCoatingEnhanced", new EntityCoordinates(target, Vector2.Zero));
    }

    public override void Update(float frameTime)
    {
        var time = _timing.CurTime;
        var charging = EntityQueryEnumerator<XenomorphSpitterComponent>();
        while (charging.MoveNext(out var uid, out var comp))
        {
            if (!comp.Charging || time < comp.ExpiresAt)
                continue;
            comp.Charging = false;
            Dirty(uid, comp);
            _movement.RefreshMovementSpeedModifiers(uid);
            _armor.RefreshAlert(uid);
            foreach (var action in _actions.GetActions(uid))
                if (_actions.GetEvent(action) is XenomorphChargeSpitActionEvent)
                    _actions.SetToggled(action.Owner, false);
        }
        var acided = EntityQueryEnumerator<XenomorphAcidedComponent>();
        while (acided.MoveNext(out var uid, out var acid))
        {
            if (time >= acid.ExpiresAt || _mobs.IsDead(uid))
            {
                RemCompDeferred<XenomorphAcidedComponent>(uid);
                continue;
            }
            if (time < acid.NextDamageAt)
                continue;
            acid.NextDamageAt += TimeSpan.FromSeconds(1);
            _damage.TryChangeDamage(uid, new DamageSpecifier { DamageDict = { ["Caustic"] = acid.Structure ? 5 : acid.Combo ? 3 : acid.DamagePerTick }, ArmorPenetration = acid.ArmorPenetration });
        }
        for (var i = _sprays.Count - 1; i >= 0; i--)
        {
            var spray = _sprays[i];
            for (var j = spray.Barricades.Count - 1; j >= 0; j--)
            {
                var barricade = spray.Barricades[j];
                if (time < barricade.At)
                    continue;
                spray.Barricades.RemoveAt(j);
                if (!Exists(barricade.Target) || !Exists(spray.User)
                    || !_interaction.InRangeUnobstructed(spray.User, barricade.Target, range: spray.Range + 1))
                    continue;
                var coating = EnsureComp<XenomorphAcidedComponent>(barricade.Target);
                coating.Structure = true;
                coating.ExpiresAt = time + TimeSpan.FromSeconds(20);
                coating.NextDamageAt = time;
                if (coating.Effect == null || !Exists(coating.Effect.Value))
                    coating.Effect = Spawn("DumontXenomorphBarricadeAcid", new EntityCoordinates(barricade.Target, Vector2.Zero));
            }
            for (var j = spray.Tiles.Count - 1; j >= 0; j--)
            {
                var tile = spray.Tiles[j];
                if (time < tile.At)
                    continue;
                spray.Tiles.RemoveAt(j);
                if (!Exists(spray.Start.EntityId) || !Exists(spray.User)
                    || !_interaction.InRangeUnobstructed(_transform.ToMapCoordinates(spray.Start), _transform.ToMapCoordinates(tile.Coordinates), range: spray.Range + 1))
                    continue;
                var pool = Spawn(spray.Stacking ? "DumontXenomorphStackingAcidSpray" : spray.Strong ? "DumontXenomorphStrongAcidSpray" : "DumontXenomorphAcidSpray", tile.Coordinates);
                spray.Pools.Add((pool, time + TimeSpan.FromSeconds(spray.Strong || spray.Stacking ? 1 : 0.6)));
                _atmos.HotspotExtinguish(tile.Coordinates.EntityId, new Vector2i((int)MathF.Floor(tile.Coordinates.X), (int)MathF.Floor(tile.Coordinates.Y)));
            }
            for (var j = spray.Pools.Count - 1; j >= 0; j--)
            {
                var pool = spray.Pools[j];
                if (!Exists(pool.Effect) || time >= pool.Until)
                {
                    if (Exists(pool.Effect))
                        QueueDel(pool.Effect);
                    spray.Pools.RemoveAt(j);
                    continue;
                }
                _nearby.Clear();
                _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(pool.Effect), 0.55f, _nearby);
                foreach (var target in _nearby)
                    SprayHit(spray, pool.Effect, target);
            }
            if (spray.Tiles.Count == 0 && spray.Pools.Count == 0 && spray.Barricades.Count == 0)
                _sprays.RemoveAt(i);
        }
    }
}
