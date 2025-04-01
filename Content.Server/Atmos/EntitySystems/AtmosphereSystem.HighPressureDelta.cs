// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2022 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Tomeno <Tomeno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Tomeno <tomeno@lulzsec.co.uk>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Armok <155400926+ARMOKS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 PuroSlavKing <103608145+PuroSlavKing@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 lzk <124214523+lzk228@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Humanoid;
using Content.Shared.Maps;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Server.Atmos.EntitySystems;

public sealed partial class AtmosphereSystem
{
    [ValidatePrototypeId<EntityPrototype>] private const string _spaceWindProto = "SpaceWindVisual"; // Backmen

    private readonly HashSet<Entity<MovedByPressureComponent>> _activePressures = new();

    private void UpdateHighPressure(float frameTime)
    {
        foreach (var ent in _activePressures)
        {
            if (!ent.Comp.Throwing || _gameTiming.CurTime < ent.Comp.ThrowingCutoffTarget
                || !TryComp(ent.Owner, out PhysicsComponent? physics))
                continue;

            if (TryComp(ent.Owner, out ThrownItemComponent? thrown))
            {
                _thrown.LandComponent(ent.Owner, thrown, physics, true);
                _thrown.StopThrow(ent.Owner, thrown);
            }

            _physics.SetBodyStatus(ent.Owner, physics, BodyStatus.OnGround);
            _physics.SetSleepingAllowed(ent.Owner, physics, true);

            ent.Comp.Throwing = false;
            _activePressures.Remove(ent);
        }
    }

    private void HighPressureMovements(Entity<GridAtmosphereComponent> gridAtmosphere,
        TileAtmosphere tile,
        EntityQuery<PhysicsComponent> bodies,
        EntityQuery<TransformComponent> xforms,
        EntityQuery<MovedByPressureComponent> pressureQuery,
        EntityQuery<MetaDataComponent> metas,
        EntityQuery<ProjectileComponent> projectileQuery,
        double gravity)
    {
        var atmosComp = gridAtmosphere.Comp;
        var oneAtmos = Atmospherics.OneAtmosphere;

        // No atmos yeets, return early.
        if (!SpaceWind
            || !gridAtmosphere.Comp.SpaceWindSimulation // Is the grid marked as exempt from space wind?
            || tile.Air is null || tile.Space // No Air Checks. Pressure differentials can't exist in a hard vacuum.
            || tile.Air.Pressure <= atmosComp.PressureCutoff // Below 5kpa(can't throw a base item)
            || oneAtmos - atmosComp.PressureCutoff <= tile.Air.Pressure
            && tile.Air.Pressure <= oneAtmos + atmosComp.PressureCutoff // Check within 5kpa of default pressure.
            || !TryComp(gridAtmosphere.Owner, out MapGridComponent? mapGrid)
            || !_mapSystem.TryGetTileRef(gridAtmosphere.Owner, mapGrid, tile.GridIndices, out var tileRef))
            return;

        var tileDef = (ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId];
        if (!tileDef.SimulatedTurf)
            return;

        var partialFrictionComposition = gravity * tileDef.MobFrictionNoInput;

        var pressureVector = GetPressureVectorFromTile(gridAtmosphere, tile);
        if (!pressureVector.IsValid())
            return;
        tile.LastPressureDirection = pressureVector;

        // Calculate this HERE so that we aren't running the square root of a whole Newton vector per item.
        var pVecLength = pressureVector.Length();
        if (pVecLength <= 1) // Then guard against extremely small vectors.
            return;

        pressureVector *= SpaceWindStrengthMultiplier;

        if (pVecLength > 15 && !tile.Hotspot.Valid && atmosComp.SpaceWindSoundCooldown == 0)
        {
            var coordinates = _mapSystem.ToCenterCoordinates(tile.GridIndex, tile.GridIndices);
            var volume = Math.Clamp(pVecLength / atmosComp.SpaceWindSoundDenominator, atmosComp.SpaceWindSoundMinVolume, atmosComp.SpaceWindSoundMaxVolume);
            _audio.PlayPvs(atmosComp.SpaceWindSound, coordinates, AudioParams.Default.WithVariation(0.125f).WithVolume(volume));

            // Backmen-Start | Space Wind Visuals
            if (SpaceWindVisuals)
            {
                var visualEnt = Spawn(_spaceWindProto, coordinates);
                var gridRotation = _transformSystem.GetWorldRotation(gridAtmosphere);
                var windAngle = pressureVector.ToAngle() + gridRotation;
                _transformSystem.SetLocalRotation(visualEnt, windAngle - MathF.PI / 2);
            }
            // Backmen-End
        }

        if (atmosComp.SpaceWindSoundCooldown++ > atmosComp.SpaceWindSoundCooldownCycles)
            atmosComp.SpaceWindSoundCooldown = 0;

        // TODO: Deprecated for now, it sucks ass and I'm disassembling monstermos because it sucks. This'll be handled by Space Wind after I'm done whiteboarding better equations for it.
        // - TCJ
        // HandleDecompressionFloorRip(mapGrid, otherTile, otherTile.PressureDifference);

        _entSet.Clear();
        _lookup.GetLocalEntitiesIntersecting(tile.GridIndex, tile.GridIndices, _entSet, 0f);

        foreach (var entity in _entSet)
        {
            // Ideally containers would have their own EntityQuery internally or something given recursively it may need to slam GetComp<T> anyway.
            // Also, don't care about static bodies (but also due to collisionwakestate can't query dynamic directly atm).
            if (!bodies.TryGetComponent(entity, out var body)
                || !pressureQuery.TryGetComponent(entity, out var pressure)
                || !pressure.Enabled
                || _containers.IsEntityInContainer(entity, metas.GetComponent(entity))
                || pressure.LastHighPressureMovementAirCycle >= gridAtmosphere.Comp.UpdateCounter)
                continue;

            // tl;dr YEET
            ExperiencePressureDifference(
                (entity, pressure),
                gridAtmosphere.Comp.UpdateCounter,
                pressureVector,
                pVecLength,
                partialFrictionComposition,
                projectileQuery,
                xforms.GetComponent(entity),
                body);
        }
    }

    // Called from AtmosphereSystem.LINDA.cs with SpaceWind CVar check handled there.
    private void ConsiderPressureDifference(GridAtmosphereComponent gridAtmosphere, TileAtmosphere tile) => gridAtmosphere.HighPressureDelta.Add(tile);

    public void ExperiencePressureDifference(Entity<MovedByPressureComponent> ent,
        int cycle,
        Vector2 pressureVector,
        float pVecLength,
        double partialFrictionComposition,
        EntityQuery<ProjectileComponent> projectileQuery,
        TransformComponent? xform = null,
        PhysicsComponent? physics = null)
    {
        var (uid, component) = ent;
        if (!Resolve(uid, ref physics, false)
            || !Resolve(uid, ref xform)
            || physics.BodyType == BodyType.Static
            || physics.LinearVelocity.Length() >= SpaceWindMaxForce)
            return;

        var alwaysThrow = partialFrictionComposition == 0 || physics.BodyStatus == BodyStatus.InAir;

        // Coefficient of static friction in Newtons (kg * m/s^2), which might not apply under certain conditions.
        var coefficientOfFriction = partialFrictionComposition * physics.Mass;
        coefficientOfFriction *= _standingSystem.IsDown(uid) ? 3 : 1;

        if (HasComp<HumanoidAppearanceComponent>(ent))
            pressureVector *= HumanoidThrowMultiplier;
        if (!alwaysThrow && pVecLength < coefficientOfFriction)
            return;

        // Yes this technically increases the magnitude by a small amount... I detest having to swap between "World" and "Local" vectors.
        // ThrowingSystem increments linear velocity by a given vector, but we have to do this anyways because reasons.
        var velocity = _transformSystem.GetWorldRotation(uid).ToWorldVec() + pressureVector;

        _sharedStunSystem.TryKnockdown(uid, TimeSpan.FromSeconds(SpaceWindKnockdownTime), false);
        _throwing.TryThrow(uid, velocity, physics, xform, projectileQuery,
            1, doSpin: physics.AngularVelocity < SpaceWindMaxAngularVelocity);

        component.LastHighPressureMovementAirCycle = cycle;
        component.Throwing = true;
        component.ThrowingCutoffTarget = _gameTiming.CurTime + component.CutoffTime;
        _activePressures.Add(ent);
    }
}
