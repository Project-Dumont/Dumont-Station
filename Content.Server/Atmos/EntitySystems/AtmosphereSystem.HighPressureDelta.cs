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

using System.Numerics;
using Content.Server.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Gravity;
using Content.Shared.Humanoid;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Atmos.EntitySystems;

public sealed partial class AtmosphereSystem
{
    [ValidatePrototypeId<EntityPrototype>] private const string _spaceWindProto = "SpaceWindVisual"; // Backmen

    private static readonly ProtoId<SoundCollectionPrototype> DefaultSpaceWindSounds = "SpaceWind";

    private const int SpaceWindSoundCooldownCycles = 75;

    private int _spaceWindSoundCooldown = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier? SpaceWindSound { get; private set; } = new SoundCollectionSpecifier(DefaultSpaceWindSounds, AudioParams.Default.WithVariation(0.125f));

    private readonly HashSet<Entity<MovedByPressureComponent>> _activePressures = new(8);

    private void UpdateHighPressure(float frameTime)
    {
        var toRemove = new RemQueue<Entity<MovedByPressureComponent>>();

        foreach (var ent in _activePressures)
        {
            var (uid, comp) = ent;
            MetaDataComponent? metadata = null;

            if (Deleted(uid, metadata))
            {
                toRemove.Add((uid, comp));
                continue;
            }

            if (Paused(uid, metadata))
                continue;

            comp.Accumulator += frameTime;

            if (comp.Accumulator < 2f)
                continue;

            // Reset it just for VV reasons even though it doesn't matter
            comp.Accumulator = 0f;
            toRemove.Add(ent);

            if (TryComp<PhysicsComponent>(uid, out var body))
            {
                _physics.SetBodyStatus(uid, body, BodyStatus.OnGround);
            }

            if (TryComp<FixturesComponent>(uid, out var fixtures)
                && TryComp<MovedByPressureComponent>(uid, out var component))
            {
                foreach (var (id, fixture) in fixtures.Fixtures)
                {
                    if (component.TableLayerRemoved.Contains(id))
                    {
                        _physics.AddCollisionMask(uid, id, fixture, (int) CollisionGroup.TableLayer, manager: fixtures);
                    }
                }
            }
        }

        foreach (var comp in toRemove)
        {
            _activePressures.Remove(comp);
        }
    }

    private void AddMobMovedByPressure(EntityUid uid, MovedByPressureComponent component, PhysicsComponent body)
    {
        if (!TryComp<FixturesComponent>(uid, out var fixtures))
            return;

        _physics.SetBodyStatus(uid, body, BodyStatus.InAir);

        foreach (var (id, fixture) in fixtures.Fixtures)
        {
            // Mark fixtures that have TableLayer removed
            if ((fixture.CollisionMask & (int) CollisionGroup.TableLayer) != 0)
            {
                component.TableLayerRemoved.Add(id);
                _physics.RemoveCollisionMask(uid, id, fixture, (int) CollisionGroup.TableLayer, manager: fixtures);
            }
        }
        // TODO: Make them dynamic type? Ehh but they still want movement so uhh make it non-predicted like weightless?
        // idk it's hard.

        component.Accumulator = 0f;
        _activePressures.Add((uid, component));
    }

    private void HighPressureMovements(Entity<GridAtmosphereComponent> gridAtmosphere,
        TileAtmosphere tile,
        EntityQuery<PhysicsComponent> bodies,
        EntityQuery<TransformComponent> xforms,
        EntityQuery<MovedByPressureComponent> pressureQuery,
        EntityQuery<MetaDataComponent> metas,
        EntityQuery<ProjectileComponent> projectileQuery,
        float frameTime)
    {
        // No atmos yeets, return early.
        if (!SpaceWind
            || tile.PressureDirection is AtmosDirection.Invalid
            || tile.Air is null
            || !TryComp(gridAtmosphere.Owner, out MapGridComponent? mapGrid)
            || !TryComp(gridAtmosphere.Owner, out GravityComponent? gravity)
            || !_mapSystem.TryGetTileRef(gridAtmosphere.Owner, mapGrid, tile.GridIndices, out var tileRef))
            return;

        var pressureVector = GetPressureVectorFromTile(gridAtmosphere, tile);
        if (!pressureVector.IsValid()
            || pressureVector.Length() <= 1) // Safeguard against "Extremely small vectors"
            return;

        // Doing this here because throwing system iterates the entire projectile list per throw. We iterate it FIRST before we try to throw things.
        var tileDef = (ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId];
        pressureVector *= SpaceWindStrengthMultiplier;

        if (pressureVector.Length() > 15 && !tile.Hotspot.Valid)
        {
            if (_spaceWindSoundCooldown == 0 && SpaceWindSound != null)
            {
                var coordinates = _mapSystem.ToCenterCoordinates(tile.GridIndex, tile.GridIndices);
                _audio.PlayPvs(SpaceWindSound, coordinates, SpaceWindSound.Params.WithVolume(MathHelper.Clamp(pressureVector.Length() / 10, 10, 100)));
            }

            // Backmen-Start | Space Wind Visuals
            if (SpaceWindVisuals && _spaceWindSoundCooldown == 0)
            {
                var location = _mapSystem.ToCenterCoordinates(tile.GridIndex, tile.GridIndices);
                var visualEnt = SpawnAtPosition(_spaceWindProto, location);
                var gridRotation = _transformSystem.GetWorldRotation(gridAtmosphere);
                var windAngle = tile.PressureDirection.ToAngle() + gridRotation;
                _transformSystem.SetLocalRotation(visualEnt, windAngle - MathF.PI / 2);
            }
            // Backmen-End
        }

        if (_spaceWindSoundCooldown++ > SpaceWindSoundCooldownCycles)
            _spaceWindSoundCooldown = 0;

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
                (entity, EnsureComp<MovedByPressureComponent>(entity)),
                gridAtmosphere.Comp.UpdateCounter,
                pressureVector,
                tileDef,
                gravity,
                projectileQuery,
                frameTime,
                xforms.GetComponent(entity),
                body);
        }
    }

    // Called from AtmosphereSystem.LINDA.cs with SpaceWind CVar check handled there.
    private void ConsiderPressureDifference(GridAtmosphereComponent gridAtmosphere, TileAtmosphere tile, AtmosDirection differenceDirection, float difference)
    {
        gridAtmosphere.HighPressureDelta.Add(tile);

        if (difference <= tile.PressureDifference)
            return;

        tile.PressureDifference = difference;
        tile.PressureDirection = differenceDirection;
    }

    public void ExperiencePressureDifference(Entity<MovedByPressureComponent> ent,
        int cycle,
        Vector2 pressureVector,
        ContentTileDefinition tile,
        GravityComponent gravity,
        EntityQuery<ProjectileComponent> projectileQuery,
        float frameTime,
        TransformComponent? xform = null,
        PhysicsComponent? physics = null)
    {
        var (uid, component) = ent;
        if (!Resolve(uid, ref physics, false)
            || !Resolve(uid, ref xform)
            || physics.BodyType == BodyType.Static
            || float.IsPositiveInfinity(component.MoveResist)
            || physics.LinearVelocity.Length() >= SpaceWindMaxVelocity)
            return;

        // Coefficient of static friction in Newtons (kg * m/s^2), which might not apply under certain conditions.
        var alwaysThrow = !gravity.Enabled || physics.BodyStatus == BodyStatus.InAir;
        var coefficientOfFriction = gravity.Acceleration * physics.Mass * tile.MobFrictionNoInput;
        coefficientOfFriction *= _standingSystem.IsDown(uid) ? 3 : 1;

        if (HasComp<HumanoidAppearanceComponent>(ent))
            pressureVector *= HumanoidThrowMultiplier;
        if (!alwaysThrow && pressureVector.Length() < coefficientOfFriction)
            return;

        var velocity = _transformSystem.GetWorldRotation(uid).ToWorldVec() - pressureVector;

        _sharedStunSystem.TryKnockdown(uid, TimeSpan.FromSeconds(SpaceWindKnockdownTime), false);
        _throwing.TryThrow(uid, -velocity, physics, xform, projectileQuery,
            pressureVector.Length(), doSpin: physics.AngularVelocity < SpaceWindMaxAngularVelocity);
        component.LastHighPressureMovementAirCycle = cycle;
    }
}
