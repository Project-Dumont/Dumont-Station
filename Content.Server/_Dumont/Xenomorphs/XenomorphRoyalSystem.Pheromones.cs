using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphRoyalSystem
{
    [Dependency] private MovementSpeedModifierSystem _movement = default!;
    private readonly Dictionary<EntityUid, string> _pheromones = new();
    private readonly Dictionary<EntityUid, (float Frenzy, float Warding, float Recovery)> _receivers = new();
    private readonly Dictionary<EntityUid, string> _sources = new();
    private TimeSpan _nextPheromones;

    public bool ChoosePheromones(EntityUid queen, string mode)
    {
        if (!TryComp<XenomorphPheromonesComponent>(queen, out var emitter)) return false;
        if (mode == "stopphero") { _pheromones.Remove(queen); SetPheromonesActive(queen, false); return true; }
        if (mode is not ("frenzy" or "warding" or "recovery") || !_mobs.IsAlive(queen) || !_plasma.ChangePlasmaAmount(queen, -emitter.PlasmaCost)) return false;
        _pheromones[queen] = mode;
        SetPheromonesActive(queen, true);
        return true;
    }
    private void SetPheromonesActive(EntityUid queen, bool active)
    {
        if (TryComp<XenomorphPheromonesComponent>(queen, out var emitter))
        { emitter.ActiveMode = active ? _pheromones.GetValueOrDefault(queen, "") : ""; Dirty(queen, emitter); }
        foreach (var action in _actions.GetActions(queen))
            if (_actions.GetEvent(action) is XenomorphPheromonesActionEvent or XenomorphQueenMessageActionEvent { Mode: "pheromones" }) _actions.SetToggled(action.Owner, active);
    }
    private void SpreadPheromones(EntityUid queen, EntityUid source, string mode)
    {
        _sources[source] = mode;
        _allies.Clear();
        var emitter = Comp<XenomorphPheromonesComponent>(queen);
        _lookup.GetEntitiesInRange(Transform(source).Coordinates, emitter.Range, _allies);
        foreach (var sister in _allies)
        {
            if (!Sister(queen, sister)) continue;
            var value = _receivers.GetValueOrDefault(sister);
            if (mode == "frenzy") value.Frenzy = Math.Max(value.Frenzy, emitter.Multiplier);
            if (mode == "warding") value.Warding = Math.Max(value.Warding, emitter.Multiplier);
            if (mode == "recovery") value.Recovery = Math.Max(value.Recovery, emitter.Multiplier);
            _receivers[sister] = value;
        }
    }
    private void UpdatePheromones()
    {
        if (_nextPheromones > _timing.CurTime) return;
        _nextPheromones = _timing.CurTime + TimeSpan.FromSeconds(1);
        _receivers.Clear();
        _sources.Clear();
        foreach (var (queen, mode) in _pheromones.ToArray())
        {
            if (!Exists(queen) || !TryComp<XenomorphPheromonesComponent>(queen, out var emitter) || !_mobs.IsAlive(queen) || !_plasma.ChangePlasmaAmount(queen, -emitter.PlasmaUpkeep))
            { _pheromones.Remove(queen); if (Exists(queen)) SetPheromonesActive(queen, false); continue; }
            SpreadPheromones(queen, queen, mode);
            if (Attached(queen) && TryComp<XenomorphRoyalComponent>(queen, out var royal))
                foreach (var leader in royal.Leaders)
                    if (Exists(leader) && Sister(queen, leader)) SpreadPheromones(queen, leader, mode);
        }
        var old = EntityQueryEnumerator<XenomorphPheromoneEffectComponent>();
        while (old.MoveNext(out var uid, out var effect))
            if (!_receivers.ContainsKey(uid)) RemCompDeferred<XenomorphPheromoneEffectComponent>(uid);
        foreach (var (uid, value) in _receivers)
        {
            var effect = EnsureComp<XenomorphPheromoneEffectComponent>(uid);
            effect.Frenzy = value.Frenzy;effect.Warding = value.Warding;effect.Recovery = value.Recovery;
            effect.Source = _sources.GetValueOrDefault(uid, "");
            Dirty(uid, effect);
            _movement.RefreshMovementSpeedModifiers(uid);
            if (value.Recovery > 0 && Comp<XenomorphComponent>(uid).OnWeed && !OnFire(uid))
            {
                if (_thresholds.TryGetThresholdForState(uid, MobState.Critical, out var threshold)) Heal(uid, threshold!.Value / 65 * value.Recovery / 2);
                if (TryComp<PlasmaVesselComponent>(uid, out var plasma)) _plasma.ChangePlasmaAmount(uid, plasma.PlasmaPerSecondOnWeed * value.Recovery / 4);
            }
        }
    }
}
