// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Examine;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Shared.Emp;

public abstract partial class SharedEmpSystem : EntitySystem // Goob - partial
{
    [Dependency] protected readonly IGameTiming Timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmpDisabledComponent, ExaminedEvent>(OnExamine);
    }

    protected const string EmpDisabledEffectPrototype = "EffectEmpDisabled";

    /// <summary>
    /// Triggers an EMP pulse at the given location, by first raising an <see cref="EmpAttemptEvent"/>, then a raising <see cref="EmpPulseEvent"/> on all entities in range.
    /// </summary>
    /// <param name="coordinates">The location to trigger the EMP pulse at.</param>
    /// <param name="range">The range of the EMP pulse.</param>
    /// <param name="energyConsumption">The amount of energy consumed by the EMP pulse.</param>
    /// <param name="duration">The duration of the EMP effects.</param>
    /// <param name="user">The player that caused the effect. Used for predicted audio.</param>
    public void EmpPulse(EntityCoordinates coordinates, float range, float energyConsumption, TimeSpan duration, EntityUid? user = null)
    {
        _entSet.Clear();
        _lookup.GetEntitiesInRange(coordinates, range, _entSet);
        foreach (var uid in _entSet)
        {
            TryEmpEffects(uid, energyConsumption, duration, user);
        }
        // TODO: replace with PredictedSpawn once it works with animated sprites
        if (_net.IsServer)
            Spawn(EmpPulseEffectPrototype, coordinates);

        _audio.PlayPredicted(EmpSound, coordinates, user);
    }

    /// <summary>
    /// Attempts to apply the effects of an EMP pulse onto an entity by first raising an <see cref="EmpAttemptEvent"/>, followed by raising a <see cref="EmpPulseEvent"/> on it.
    /// </summary>
    /// <param name="uid">The entity to apply the EMP effects on.</param>
    /// <param name="energyConsumption">The amount of energy consumed by the EMP.</param>
    /// <param name="duration">The duration of the EMP effects.</param>
    /// <param name="user">The player that caused the EMP. For prediction purposes.</param>
    /// <returns>If the entity was affected by the EMP.</returns>
    public bool TryEmpEffects(EntityUid uid, float energyConsumption, TimeSpan duration, EntityUid? user = null)
    {
        var attemptEv = new EmpAttemptEvent();
        RaiseLocalEvent(uid, ref attemptEv);
        if (attemptEv.Cancelled)
            return false;

        if (IsEmpWardedByContainer(uid)) // Goob
            return false;

        return DoEmpEffects(uid, energyConsumption, duration, user);
    }

    /// <summary>
    /// Applies the effects of an EMP pulse onto an entity by raising a <see cref="EmpPulseEvent"/> on it.
    /// </summary>
    /// <param name="uid">The entity to apply the EMP effects on.</param>
    /// <param name="energyConsumption">The amount of energy consumed by the EMP.</param>
    /// <param name="duration">The duration of the EMP effects.</param>
    /// <param name="user">The player that caused the EMP. For prediction purposes.</param>
    /// <returns>If the entity was affected by the EMP.</returns>
    public bool DoEmpEffects(EntityUid uid, float energyConsumption, TimeSpan duration, EntityUid? user = null)
    {
        var strMultiplier = 1f;
        var durMultiplier = 1f;
        if (_resistanceQuery.TryComp(uid, out var resistance))
        {
            strMultiplier = resistance.StrengthMultiplier;
            durMultiplier = resistance.DurationMultiplier;
        }
        var ev = new EmpPulseEvent(energyConsumption * strMultiplier, false, false, duration * durMultiplier, user);
        RaiseLocalEvent(uid, ref ev);

        // TODO: replace with PredictedSpawn once it works with animated sprites
        if (ev.Affected && _net.IsServer)
            Spawn(EmpDisabledEffectPrototype, Transform(uid).Coordinates);

        if (!ev.Disabled)
            return ev.Affected;

        var disabled = EnsureComp<EmpDisabledComponent>(uid);
        disabled.DisabledUntil = Timing.CurTime + duration * durMultiplier;
        Dirty(uid, disabled);

        return ev.Affected;
    }

    public override void Update(float frameTime)
    public virtual void EmpPulse(MapCoordinates coordinates, float range, float energyConsumption, float duration)
    {
    }

    private void OnExamine(Entity<EmpDisabledComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("emp-disabled-comp-on-examine"));
    }
}
