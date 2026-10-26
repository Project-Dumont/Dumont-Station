using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphDespoilerSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedInteractionSystem _interaction = default!;
    [Dependency] private SharedGunSystem _guns = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    [Dependency] private XenomorphArmorSystem _armor = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    private readonly HashSet<Entity<MobStateComponent>> _targets = new();
    private readonly List<WaveTile> _waves = new();
    private readonly Dictionary<EntityUid, EntityUid> _visuals = new();
    private readonly Dictionary<EntityUid, TimeSpan> _immunity = new();
    private sealed record WaveTile(EntityUid Caster, EntityCoordinates Origin, EntityCoordinates Tile,
        TimeSpan At, bool Empowered, HashSet<EntityUid> Hit);

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphDespoilerComponent, XenomorphAcidBarrageActionEvent>(OnBarrage);
        SubscribeLocalEvent<XenomorphDespoilerComponent, XenomorphCausticEmbraceActionEvent>(OnEmbrace);
        SubscribeLocalEvent<XenomorphDespoilerComponent, XenomorphOozingWoundsActionEvent>(OnWounds);
        SubscribeLocalEvent<XenomorphDespoilerComponent, XenomorphCatalyzeActionEvent>(OnCatalyze);
        SubscribeLocalEvent<XenomorphDespoilerComponent, DamageChangedEvent>(OnDamage);
        SubscribeLocalEvent<XenomorphDespoilerComponent, MeleeHitEvent>(OnMelee);
        SubscribeLocalEvent<XenomorphDespoilerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<XenomorphDespoilerPoolComponent, MapInitEvent>(OnPoolInit);
        SubscribeLocalEvent<XenomorphDespoilerPoolComponent, StartCollideEvent>(OnPoolCollide);
        SubscribeLocalEvent<XenomorphDespoilerProjectileComponent, EntityTerminatingEvent>(OnProjectileEnd);
        SubscribeLocalEvent<XenomorphDespoilerProjectileComponent, PreventCollideEvent>(OnProjectileCollide);
        SubscribeLocalEvent<XenomorphCausticLeapComponent, StartCollideEvent>(OnLeapCollide);
        SubscribeNetworkEvent<XenomorphBarrageInputEvent>(OnInput);
    }
    private bool Ready(EntityUid uid) => _mobs.IsAlive(uid) && _blocker.CanAttack(uid);
    private bool Victim(EntityUid uid) => Exists(uid) && HasComp<MobStateComponent>(uid)
        && !HasComp<XenomorphComponent>(uid) && !HasComp<SiliconComponent>(uid) && !_mobs.IsDead(uid);
    private void Sound(EntityUid uid, string path) => _audio.PlayPvs(new SoundPathSpecifier(path), uid);
    private void Refresh(Entity<XenomorphDespoilerComponent> ent)
    {
        Dirty(ent);
        _armor.RefreshAlert(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
    }
    private void AddPoints(Entity<XenomorphDespoilerComponent> ent, float points)
    {
        ent.Comp.Points += points;
        ent.Comp.LastActivityAt = _timing.CurTime;
        while (ent.Comp.Points >= 200 && ent.Comp.Stacks < 4)
        {
            ent.Comp.Points -= 200;
            ent.Comp.Stacks++;
        }
        if (ent.Comp.Stacks == 4)
            ent.Comp.Points = 0;
        Refresh(ent);
    }
    private void OnDamage(Entity<XenomorphDespoilerComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageIncreased && args.DamageDelta is { } delta)
            AddPoints(ent, delta.GetTotal().Float() * 0.5f);
    }
    private void OnMelee(Entity<XenomorphDespoilerComponent> ent, ref MeleeHitEvent args)
    {
        foreach (var target in args.HitEntities)
        {
            if (!Victim(target))
                continue;
            if (ent.Comp.Stacks >= 3)
                ApplyAcid(target, 1);
            AddPoints(ent, 200);
        }
    }
    public DamageSpecifier TailBonus(Entity<XenomorphDespoilerComponent> ent, EntityUid target)
    {
        var amount = ent.Comp.Stacks * 20;
        if (TryComp<XenomorphAcidedComponent>(target, out var acid) && acid.FinishingTier > 0)
        {
            amount += Math.Clamp(acid.FinishingTier, 1, 4) * 10;
            acid.FinishingTier--;
        }
        return new DamageSpecifier { DamageDict = { ["Caustic"] = amount } };
    }
    public void ApplyAcid(EntityUid target, int tier)
    {
        if (!Victim(target))
            return;
        tier = Math.Clamp(tier, 1, 3);
        var acid = EnsureComp<XenomorphAcidedComponent>(target);
        if (acid.Tier > tier && acid.ExpiresAt > _timing.CurTime)
            return;
        acid.Tier = tier;
        acid.FinishingTier = Math.Max(acid.FinishingTier, tier + 1);
        acid.DamagePerTick = tier switch { 2 => 2.5f, 3 => 6.55f, _ => 1 };
        acid.Combo = false;
        acid.Resists = tier == 1 ? 1 : 2;
        acid.ExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(tier == 1 ? 20 : 40);
        acid.NextDamageAt = _timing.CurTime;
        if (acid.Effect is { } old && Exists(old))
            QueueDel(old);
        acid.Effect = Spawn(tier == 1 ? "DumontXenomorphAcidCoating" : "DumontXenomorphAcidCoatingEnhanced", new EntityCoordinates(target, Vector2.Zero));
    }
    private void OnCatalyze(Entity<XenomorphDespoilerComponent> ent, ref XenomorphCatalyzeActionEvent args)
    {
        if (args.Handled || !Ready(ent))
            return;
        if (ent.Comp.Stacks < 1)
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-despoiler-no-hypertension"), ent, ent);
            return;
        }
        ent.Comp.Stacks--;
        ent.Comp.Empowered = true;
        ent.Comp.EmpowerExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(15);
        ClearVisual(ent);
        _visuals[ent] = Spawn("DumontDespoilerCatalyze", new EntityCoordinates(ent, Vector2.Zero));
        _actions.SetToggled(args.Action.Owner, true);
        Refresh(ent);
        args.Handled = true;
    }
    private bool TakeEmpowerment(Entity<XenomorphDespoilerComponent> ent)
    {
        var active = ent.Comp.Empowered && _timing.CurTime < ent.Comp.EmpowerExpiresAt;
        ent.Comp.Empowered = false;
        ClearVisual(ent);
        foreach (var action in _actions.GetActions(ent))
            if (_actions.GetEvent(action) is XenomorphCatalyzeActionEvent)
                _actions.SetToggled(action.Owner, false);
        Dirty(ent);
        return active;
    }
    private void ClearVisual(EntityUid uid)
    {
        if (_visuals.Remove(uid, out var visual) && Exists(visual))
            QueueDel(visual);
    }
    private void OnShutdown(Entity<XenomorphDespoilerComponent> ent, ref ComponentShutdown args) => ClearVisual(ent);
    private void OnBarrage(Entity<XenomorphDespoilerComponent> ent, ref XenomorphAcidBarrageActionEvent args)
    {
        if (args.Handled || !Ready(ent))
            return;
        ent.Comp.Armed = !ent.Comp.Armed;
        ent.Comp.Charging = false;
        _actions.SetToggled(args.Action.Owner, ent.Comp.Armed);
        Refresh(ent);
        args.Handled = true;
    }
    private void OnInput(XenomorphBarrageInputEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } uid || !TryComp<XenomorphDespoilerComponent>(uid, out var comp))
            return;
        HandleBarrageInput(uid, GetCoordinates(msg.Target), msg.Firing);
    }
    public void HandleBarrageInput(EntityUid uid, EntityCoordinates coordinates, bool firing)
    {
        if (!TryComp<XenomorphDespoilerComponent>(uid, out var comp) || !Ready(uid) || !comp.Armed || !coordinates.IsValid(EntityManager))
            return;
        var target = _transform.ToMapCoordinates(coordinates);
        var origin = _transform.GetMapCoordinates(uid);
        if (origin.MapId != target.MapId || !float.IsFinite(target.Position.LengthSquared()))
            return;
        if (!firing)
        {
            if (comp.Charging)
                return;
            comp.Charging = true;
            comp.ChargeStartedAt = _timing.CurTime;
            Sound(uid, "/Audio/_RMC14/Xeno/acid_sizzle1.ogg");
            Refresh((uid, comp));
            return;
        }
        if (!comp.Charging)
            return;
        var direction = target.Position - origin.Position;
        if (!_plasma.HasPlasma(uid, 75) || direction.LengthSquared() < 0.001f)
        {
            comp.Charging = false;
            Refresh((uid, comp));
            return;
        }
        var charge = Math.Clamp((float)(_timing.CurTime - comp.ChargeStartedAt).TotalSeconds / 5, 0, 1);
        var count = (int)MathF.Round(1 + 9 * charge) + (TakeEmpowerment((uid, comp)) ? 5 : 0);
        direction = Vector2.Normalize(direction);
        for (var i = 0; i < count; i++)
        {
            var spread = count == 1 ? 0 : -60 + 120f * i / (count - 1);
            var shotDirection = Angle.FromDegrees(spread).RotateVec(direction);
            var spawn = new MapCoordinates(origin.Position + direction, origin.MapId);
            var projectile = Spawn("DumontDespoilerAcidShot", spawn);
            var shot = Comp<XenomorphDespoilerProjectileComponent>(projectile);
            shot.Caster = uid;
            shot.Origin = spawn;
            shot.Range = _random.Next(2, 7);
            var visual = EnsureComp<XenomorphDespoilerShotVisualComponent>(projectile);
            visual.Scale = new Vector2(_random.NextFloat(0.9f, 1.33f));
            Dirty(projectile, visual);
            _guns.ShootProjectile(projectile, shotDirection, Vector2.Zero, uid, uid, 12);
        }
        _plasma.ChangePlasmaAmount(uid, -75);
        Sound(uid, "/Audio/_RMC14/Xeno/alien_spitacid2.ogg");
        comp.Armed = comp.Charging = false;
        foreach (var action in _actions.GetActions(uid))
            if (_actions.GetEvent(action) is XenomorphAcidBarrageActionEvent)
            {
                _actions.SetToggled(action.Owner, false);
                _actions.SetCooldown(action.Owner, TimeSpan.FromSeconds(15));
            }
        Refresh((uid, comp));
    }
    private void OnProjectileCollide(Entity<XenomorphDespoilerProjectileComponent> ent, ref PreventCollideEvent args)
    {
        if (HasComp<XenomorphComponent>(args.OtherEntity))
            args.Cancelled = true;
    }
    private void OnProjectileEnd(Entity<XenomorphDespoilerProjectileComponent> ent, ref EntityTerminatingEvent args)
    {
        if (TerminatingOrDeleted(Transform(ent).ParentUid) || !Exists(ent.Comp.Caster))
            return;
        SpawnPool(ent.Comp.Caster, _transform.GetMoverCoordinates(ent), false);
    }
    private void SpawnPool(EntityUid caster, EntityCoordinates coordinates, bool spray, bool empowered = false, HashSet<EntityUid>? hits = null)
    {
        var pool = Spawn(spray ? "DumontDespoilerAcidSpray" : "DumontDespoilerLingeringAcid", coordinates);
        var comp = Comp<XenomorphDespoilerPoolComponent>(pool);
        comp.Caster = caster;
        comp.Empowered = empowered;
        if (hits != null)
            comp.Hit = hits;
    }
    private void OnPoolInit(Entity<XenomorphDespoilerPoolComponent> ent, ref MapInitEvent args)
    {
        if (!ent.Comp.Spray)
            EnsureComp<TimedDespawnComponent>(ent).Lifetime = _random.NextFloat(15, 20);
    }
    private void OnPoolCollide(Entity<XenomorphDespoilerPoolComponent> ent, ref StartCollideEvent args)
    {
        var target = args.OtherEntity;
        if (!Victim(target))
            return;
        if (!ent.Comp.Spray)
        {
            if (TryComp<PullableComponent>(target, out var pull) && pull.BeingPulled)
                return;
            _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Caustic"] = 20 } }, origin: ent.Comp.Caster);
            return;
        }
        if (_immunity.TryGetValue(target, out var immunity) && immunity > _timing.CurTime || !ent.Comp.Hit.Add(target))
            return;
        _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Caustic"] = 30 } }, origin: ent.Comp.Caster);
        ApplyAcid(target, ent.Comp.Empowered ? 2 : 1);
        if (ent.Comp.Empowered)
        {
            _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(1));
            _immunity[target] = _timing.CurTime + TimeSpan.FromSeconds(3);
        }
    }
    private void OnWounds(Entity<XenomorphDespoilerComponent> ent, ref XenomorphOozingWoundsActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !_plasma.HasPlasma(ent, 50))
            return;
        var health = TryComp<DamageableComponent>(ent, out var damage) ? 1 - Math.Clamp(damage.TotalDamage.Float() / 750, 0, 1) : 1;
        var severity = (health <= 0.7 ? 1 : 0) + (health <= 0.3 ? 1 : 0);
        ent.Comp.Stacks = Math.Max(0, ent.Comp.Stacks - (severity == 0 ? ent.Comp.Stacks : severity == 1 ? 2 : 1));
        var radius = 1 + severity;
        var empowered = TakeEmpowerment(ent);
        var origin = _transform.GetMoverCoordinates(ent);
        var sharedHits = new HashSet<EntityUid>();
        for (var x = -radius; x <= radius; x++)
        for (var y = -radius; y <= radius; y++)
        {
            if (x == 0 && y == 0 || x * x + y * y > radius * radius)
                continue;
            var tile = origin.Offset(new Vector2(x, y));
            if (!_interaction.InRangeUnobstructed(ent.Owner, tile, range: radius + 1))
                continue;
            Spawn("DumontDespoilerOozingTelegraph", tile);
            _waves.Add(new WaveTile(ent, origin, tile, _timing.CurTime + TimeSpan.FromSeconds(Math.Max(Math.Abs(x), Math.Abs(y)) * 0.2), empowered, sharedHits));
        }
        Sound(ent, "/Audio/_RMC14/Xeno/acid_sizzle3.ogg");
        Refresh(ent);
        args.Handled = true;
    }
    private void OnEmbrace(Entity<XenomorphDespoilerComponent> ent, ref XenomorphCausticEmbraceActionEvent args)
    {
        if (args.Handled || !Ready(ent) || !_plasma.HasPlasma(ent, 50) || HasComp<XenomorphCausticLeapComponent>(ent)
            || !_blocker.CanMove(ent) || Transform(ent).Anchored || _container.IsEntityInContainer(ent))
            return;
        var origin = _transform.GetMapCoordinates(ent);
        var target = _transform.ToMapCoordinates(args.Target);
        var delta = target.Position - origin.Position;
        var distance = delta.Length();
        if (origin.MapId != target.MapId || !float.IsFinite(distance) || distance < 0.01f)
            return;
        var empowered = ent.Comp.Empowered && _timing.CurTime < ent.Comp.EmpowerExpiresAt;
        EntityUid? victim = null;
        if (empowered)
        {
            if (distance > 5)
                return;
            _targets.Clear();
            _lookup.GetEntitiesInRange(target, 0.65f, _targets);
            foreach (var candidate in _targets)
                if (Victim(candidate) && _interaction.InRangeUnobstructed(ent.Owner, candidate.Owner, range: 6))
                {
                    victim = candidate;
                    break;
                }
            if (victim == null)
                return;
        }
        var direction = delta / distance;
        var destination = new MapCoordinates(origin.Position + direction * Math.Min(distance, empowered ? 5 : 3), origin.MapId);
        if (!_interaction.InRangeUnobstructed(ent.Owner, _transform.ToCoordinates(destination), range: 6) || !TryComp<PhysicsComponent>(ent, out var body))
            return;
        if (empowered)
            TakeEmpowerment(ent);
        var leap = AddComp<XenomorphCausticLeapComponent>(ent);
        leap.Victim = victim;
        leap.Empowered = empowered;
        leap.Destination = destination;
        leap.Direction = direction;
        leap.EndsAt = _timing.CurTime + TimeSpan.FromSeconds(Math.Min(distance, empowered ? 5 : 3) / 20);
        _physics.SetBodyStatus(ent, body, BodyStatus.InAir);
        _physics.SetLinearVelocity(ent, direction * 20, body: body);
        Sound(ent, "/Audio/_RMC14/Xeno/alien_roar1.ogg");
        args.Handled = true;
    }
    private void OnLeapCollide(Entity<XenomorphCausticLeapComponent> ent, ref StartCollideEvent args)
    {
        if (HasComp<XenomorphComponent>(args.OtherEntity))
            return;
        if (ent.Comp.Empowered && ent.Comp.Victim != args.OtherEntity)
            return;
        FinishLeap(ent, ent.Comp.Empowered);
    }
    private void FinishLeap(Entity<XenomorphCausticLeapComponent> ent, bool hit)
    {
        if (ent.Comp.Resolved)
            return;
        ent.Comp.Resolved = true;
        if (TryComp<PhysicsComponent>(ent, out var body))
        {
            _physics.SetLinearVelocity(ent, Vector2.Zero, body: body);
            _physics.SetBodyStatus(ent, body, BodyStatus.OnGround);
        }
        if (ent.Comp.Empowered)
        {
            if (hit && ent.Comp.Victim is { } victim && Victim(victim))
            {
                _damage.TryChangeDamage(victim, new DamageSpecifier { DamageDict = { ["Caustic"] = 30 } }, origin: ent.Owner);
                ApplyAcid(victim, 3);
                _stun.TryUpdateParalyzeDuration(victim, TimeSpan.FromSeconds(1));
            }
        }
        else
        {
            var center = _transform.GetMoverCoordinates(ent);
            var back = new Vector2i(Math.Sign(MathF.Round(-ent.Comp.Direction.X)), Math.Sign(MathF.Round(-ent.Comp.Direction.Y)));
            var hits = new HashSet<EntityUid>();
            for (var x = -1; x <= 1; x++)
            for (var y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0 || x == back.X && y == back.Y)
                    continue;
                var tile = center.Offset(new Vector2(x, y));
                if (!_interaction.InRangeUnobstructed(ent.Owner, tile, range: 3))
                    continue;
                Spawn("DumontDespoilerCausticTelegraph", tile);
                _targets.Clear();
                _lookup.GetEntitiesInRange(_transform.ToMapCoordinates(tile), 0.5f, _targets);
                foreach (var target in _targets)
                    if (Victim(target) && hits.Add(target))
                        _damage.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Caustic"] = 30 } }, origin: ent.Owner);
                if (_random.Prob(0.3f))
                    SpawnPool(ent, tile, false);
            }
        }
        RemCompDeferred<XenomorphCausticLeapComponent>(ent);
    }
    public override void Update(float frameTime)
    {
        var now = _timing.CurTime;
        foreach (var (target, expiry) in _immunity)
            if (expiry <= now || !Exists(target))
                _immunity.Remove(target);
        var query = EntityQueryEnumerator<XenomorphDespoilerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Empowered && now >= comp.EmpowerExpiresAt)
                TakeEmpowerment((uid, comp));
            if (comp.Charging && (!Ready(uid) || now > comp.ChargeStartedAt + TimeSpan.FromSeconds(35)))
            {
                comp.Armed = comp.Charging = false;
                Refresh((uid, comp));
                foreach (var action in _actions.GetActions(uid))
                    if (_actions.GetEvent(action) is XenomorphAcidBarrageActionEvent)
                        _actions.SetToggled(action.Owner, false);
            }
            if (comp.Stacks > 0 && now - comp.LastActivityAt > TimeSpan.FromSeconds(10))
            {
                comp.Points -= 120 * frameTime;
                while (comp.Points < 0 && comp.Stacks > 0)
                {
                    comp.Stacks--;
                    comp.Points += 200;
                }
                Refresh((uid, comp));
            }
        }
        var projectiles = EntityQueryEnumerator<XenomorphDespoilerProjectileComponent>();
        while (projectiles.MoveNext(out var uid, out var shot))
        {
            var position = _transform.GetMapCoordinates(uid);
            if (position.MapId != shot.Origin.MapId || Vector2.DistanceSquared(position.Position, shot.Origin.Position) >= shot.Range * shot.Range)
                QueueDel(uid);
        }
        var leaps = EntityQueryEnumerator<XenomorphCausticLeapComponent>();
        while (leaps.MoveNext(out var uid, out var leap))
        {
            var position = _transform.GetMapCoordinates(uid);
            if (position.MapId != leap.Destination.MapId || !Ready(uid))
                FinishLeap((uid, leap), false);
            else if (Vector2.Dot(position.Position - leap.Destination.Position, leap.Direction) >= 0 || now >= leap.EndsAt)
                FinishLeap((uid, leap), false);
        }
        for (var i = _waves.Count - 1; i >= 0; i--)
        {
            var wave = _waves[i];
            if (now < wave.At)
                continue;
            _waves.RemoveAt(i);
            if (!Exists(wave.Caster) || !_interaction.InRangeUnobstructed(_transform.ToMapCoordinates(wave.Origin), _transform.ToMapCoordinates(wave.Tile), range: 5))
                continue;
            SpawnPool(wave.Caster, wave.Tile, true, wave.Empowered, wave.Hit);
            if (_random.Prob(0.2f))
                SpawnPool(wave.Caster, wave.Tile, false);
        }
    }
}
