using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.ActionBlocker;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphChargeSystem : SharedXenomorphChargeSystem
{
    [Dependency] private SharedStutteringSystem _stuttering = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private MovementModStatusSystem _slow = default!;
    [Dependency] private SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IGameTiming _timing = default!;
    private readonly HashSet<(EntityUid Charger, EntityUid Target)> _hits = new();
    private readonly Dictionary<EntityUid, TimeSpan> _recentHits = new();
    private static readonly SoundSpecifier Impact = new SoundCollectionSpecifier("Punch");
    private static readonly EntProtoId Slowdown = "XenomorphBullSlowdown";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenomorphChargeComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<XenomorphChargeComponent> ent, ref StartCollideEvent args)
    {
        if (ent.Comp.Active && ent.Comp.Stage > 0
            && (args.OtherFixture.Hard || HasComp<MobStateComponent>(args.OtherEntity)))
            _hits.Add((ent, args.OtherEntity));
    }

    private void Hit(Entity<XenomorphChargeComponent> ent, EntityUid target)
    {
        if (!ent.Comp.Active || ent.Comp.Stage <= 0 || !_blocker.CanAttack(ent)
            || !Exists(target) || HasComp<XenomorphComponent>(target))
            return;
        if (!HasComp<MobStateComponent>(target))
        {
            Reset(ent);
            return;
        }
        if (_mobs.IsDead(target) || _recentHits.ContainsKey(target))
            return;
        var origin = _transform.GetMapCoordinates(ent);
        var victim = _transform.GetMapCoordinates(target);
        var direction = victim.Position - origin.Position;
        if (origin.MapId != victim.MapId || direction.LengthSquared() < 0.001f)
            return;
        _recentHits[target] = _timing.CurTime + TimeSpan.FromSeconds(1);
        var scale = Math.Clamp((float) ent.Comp.Stage / ent.Comp.MaxStage, 0, 1);
        var mode = ent.Comp.Mode;
        var amount = mode switch
        {
            XenomorphChargeMode.Headbutt => 14,
            XenomorphChargeMode.Gore => 24,
            _ => 8,
        };
        var type = mode == XenomorphChargeMode.Gore ? "Piercing" : "Blunt";
        _damage.TryChangeDamage(target, new DamageSpecifier
        {
            DamageDict = { [type] = FixedPoint2.New(amount * scale) },
        }, origin: ent);
        _audio.PlayPvs(Impact, target);
        var distance = 1.5f;
        if (mode == XenomorphChargeMode.Headbutt)
        {
            distance = 3;
            _stun.TryUpdateParalyzeDuration(target, TimeSpan.FromSeconds(2 * scale));
        }
        else if (mode == XenomorphChargeMode.Gore)
        {
            distance = 1;
            _stuttering.DoStutter(target, TimeSpan.FromSeconds(2 * scale), true);
            if (TryComp<MovementSpeedModifierComponent>(target, out var movement))
            {
                var speed = Math.Max(movement.CurrentSprintSpeed, movement.BaseSprintSpeed);
                _slow.TryUpdateMovementSpeedModDuration(target, Slowdown, TimeSpan.FromSeconds(4 * scale), 1 / (1 + speed * 0.4f));
            }
            if (_solutions.TryGetInjectableSolution(target, out Entity<SolutionComponent>? solution, out _))
            {
                var available = solution.Value.Comp.Solution.AvailableVolume;
                if (available < 10)
                    _solutions.SplitSolution(solution.Value, 10 - available);
                _solutions.TryAddReagent(solution.Value, "DumontXenomorphPainToxin", 10);
            }
        }
        if (!Transform(target).Anchored)
            _throwing.TryThrow(target, Vector2.Normalize(direction) * distance, 10, user: ent, doSpin: false);
        if (mode != XenomorphChargeMode.Plow)
            Reset(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        foreach (var (target, until) in _recentHits)
        {
            if (until <= _timing.CurTime || !Exists(target))
                _recentHits.Remove(target);
        }
        foreach (var (charger, target) in _hits)
        {
            if (TryComp<XenomorphChargeComponent>(charger, out var charge))
                Hit((charger, charge), target);
        }
        _hits.Clear();
    }
}
