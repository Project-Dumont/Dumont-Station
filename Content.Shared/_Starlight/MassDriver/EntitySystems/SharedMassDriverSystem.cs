// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.MassDriver.Components;
using Content.Shared.Audio;
using Content.Shared.Ghost;
using Content.Shared.Power;
using Content.Shared.Throwing;
using Robust.Shared.Timing;

namespace Content.Shared._Starlight.MassDriver.EntitySystems;

public abstract partial class SharedMassDriverSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _audio = default!;

    private EntityQuery<GhostComponent> _ghostQuery;
    private readonly HashSet<EntityUid> _entities = new();

    public override void Initialize()
    {
        base.Initialize();

        _ghostQuery = GetEntityQuery<GhostComponent>();
        SubscribeLocalEvent<MassDriverComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnPowerChanged(EntityUid uid, MassDriverComponent component, ref PowerChangedEvent args)
    {
        if (component.Mode != MassDriverMode.Auto)
            return;

        if (TryComp<ActiveMassDriverComponent>(uid, out var active) && !args.Powered)
        {
            StopLaunching(uid, component, active, true);
            RemComp<ActiveMassDriverComponent>(uid);
        }
        else if (active == null && args.Powered)
        {
            EnsureComp<ActiveMassDriverComponent>(uid);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActiveMassDriverComponent, MassDriverComponent>();

        while (query.MoveNext(out var uid, out var activeMassDriver, out var massDriver))
        {
            if (_timing.CurTime < activeMassDriver.NextUpdateTime ||
                activeMassDriver.NextThrowTime != TimeSpan.Zero && _timing.CurTime < activeMassDriver.NextThrowTime)
            {
                continue;
            }

            activeMassDriver.NextUpdateTime = _timing.CurTime + activeMassDriver.UpdateDelay;
            Dirty(uid, activeMassDriver);

            if (activeMassDriver.LaunchEndTime != TimeSpan.Zero)
            {
                if (_timing.CurTime < activeMassDriver.LaunchEndTime)
                    continue;

                StopLaunching(uid, massDriver, activeMassDriver);

                if (massDriver.Mode == MassDriverMode.Manual)
                {
                    RemComp<ActiveMassDriverComponent>(uid);
                    continue;
                }
            }

            _entities.Clear();
            _lookup.GetEntitiesIntersecting(uid, _entities, LookupFlags.Dynamic);

            _entities.RemoveWhere(_ghostQuery.HasComp);
            var entityCount = _entities.Count;

            if (entityCount == 0)
            {
                StopLaunching(uid, massDriver, activeMassDriver);

                if (massDriver.Mode == MassDriverMode.Manual)
                    RemComp<ActiveMassDriverComponent>(uid);

                continue;
            }

            if (activeMassDriver.NextThrowTime == TimeSpan.Zero)
            {
                activeMassDriver.NextThrowTime = _timing.CurTime + massDriver.ThrowDelay;
                Dirty(uid, activeMassDriver);
                continue;
            }

            ChangePowerLoad(uid, massDriver, massDriver.LaunchPowerLoad);
            _appearance.SetData(uid, MassDriverVisuals.Launching, true);
            activeMassDriver.NextThrowTime = TimeSpan.Zero;
            activeMassDriver.LaunchEndTime = _timing.CurTime + massDriver.LaunchAnimationTime;
            activeMassDriver.NextUpdateTime = activeMassDriver.LaunchEndTime;
            Dirty(uid, activeMassDriver);

            ThrowEntities(uid, massDriver, _entities, entityCount);

            if (TryComp<AmbientSoundComponent>(uid, out var ambientSound))
                _audio.SetAmbience(uid, true, ambientSound);
        }
    }

    private void ThrowEntities(
        EntityUid massDriver,
        MassDriverComponent massDriverComponent,
        HashSet<EntityUid> targets,
        int targetCount)
    {
        var xform = Transform(massDriver);
        var distance = massDriverComponent.CurrentThrowDistance - massDriverComponent.ThrowCountDelta * (targets.Count - 1);
        var throwing = xform.LocalRotation.ToWorldVec() * distance;
        var direction = xform.Coordinates.Offset(throwing);
        var speed = massDriverComponent.Hacked
            ? massDriverComponent.HackedSpeedRewrite
            : massDriverComponent.CurrentThrowSpeed - massDriverComponent.ThrowCountDelta * (targetCount - 1);

        foreach (var entity in targets)
            _throwing.TryThrow(entity, direction, speed);
    }

    protected void StopLaunching(
        EntityUid uid,
        MassDriverComponent massDriver,
        ActiveMassDriverComponent activeMassDriver,
        bool force = false)
    {
        if (!force &&
            activeMassDriver.NextThrowTime == TimeSpan.Zero &&
            activeMassDriver.LaunchEndTime == TimeSpan.Zero)
        {
            return;
        }

        if (TryComp<AmbientSoundComponent>(uid, out var ambient))
            _audio.SetAmbience(uid, false, ambient);

        activeMassDriver.NextThrowTime = TimeSpan.Zero;
        activeMassDriver.LaunchEndTime = TimeSpan.Zero;
        _appearance.SetData(uid, MassDriverVisuals.Launching, false);
        ChangePowerLoad(uid, massDriver, massDriver.MassDriverPowerLoad);
        Dirty(uid, activeMassDriver);
    }

    protected void StartManualLaunch(EntityUid uid, MassDriverComponent massDriver)
    {
        var activeMassDriver = EnsureComp<ActiveMassDriverComponent>(uid);
        StopLaunching(uid, massDriver, activeMassDriver, true);

        activeMassDriver.NextUpdateTime = TimeSpan.Zero;
        activeMassDriver.NextThrowTime = TimeSpan.Zero;
        activeMassDriver.LaunchEndTime = TimeSpan.Zero;
        Dirty(uid, activeMassDriver);
    }

    public abstract void ChangePowerLoad(EntityUid uid, MassDriverComponent component, float powerLoad);
}
