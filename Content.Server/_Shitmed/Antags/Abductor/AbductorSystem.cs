// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._Shitmed.Antags.Abductor;
using Content.Shared.Eye;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Pinpointer;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Interaction.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.UserInterface;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Station.Components;
using Robust.Server.GameObjects;
using Content.Shared.Tag;
using Robust.Server.Containers;
using System.Numerics;

namespace Content.Server._Shitmed.Antags.Abductor;

public sealed partial class AbductorSystem : SharedAbductorSystem
{
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly TransformSystem _xformSys = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;

    private double _updateTimer = 0.0;
    private const double UpdateInterval = 1.0; 

    public override void Initialize()
    {
        SubscribeLocalEvent<AbductorHumanObservationConsoleComponent, BeforeActivatableUIOpenEvent>(OnBeforeActivatableUIOpen);
        SubscribeLocalEvent<AbductorHumanObservationConsoleComponent, ActivatableUIOpenAttemptEvent>(OnActivatableUIOpenAttempt);
        Subs.BuiEvents<AbductorHumanObservationConsoleComponent>(AbductorCameraConsoleUIKey.Key, subs => subs.Event<AbductorBeaconChosenBuiMsg>(OnAbductorBeaconChosenBuiMsg));
        InitializeActions();
        InitializeGizmo();
        InitializeConsole();
        InitializeVest();
        InitializeVictim();
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateTimer += frameTime;

        if (_updateTimer < UpdateInterval)
            return;
        _updateTimer = 0;

        var query = _entityManager.EntityQueryEnumerator<AbductorHumanObservationConsoleComponent>();

        while (query.MoveNext(out var id, out var comp))
        {
            if (!_uiSystem.IsUiOpen(id, AbductorCameraConsoleUIKey.Key)
                || !_uiSystem.TryGetUiState<AbductorCameraConsoleBuiState>(id, AbductorCameraConsoleUIKey.Key, out var state))
                continue;

            var result = new Dictionary<NetEntity, StationBeacons>();
            var isChanged = false;

            foreach (var station in state.Stations)
            {
                var consoleTransform = Transform(id);
                var stationUid = _entityManager.GetEntity(station.Value.StationId);

                if (_stationSystem.GetLargestGrid(stationUid) is not { } gridUid)
                    continue;

                var gridTransform = Transform(gridUid);
                var isEnabled = _xformSys.InRange((id, consoleTransform), (gridUid, gridTransform), comp.MinStationDistance);

                if (isEnabled == station.Value.IsEnabled)
                    continue;

                result.Add(station.Value.StationId, new StationBeacons
                {
                    Name = station.Value.Name,
                    StationId = station.Value.StationId,
                    Beacons = [.. station.Value.Beacons],
                    IsEnabled = isEnabled,
                });

                isChanged = true;
            }

            if (isChanged)
                _uiSystem.SetUiState(id, AbductorCameraConsoleUIKey.Key, new AbductorCameraConsoleBuiState() { Stations = result });
        }
    }

    private void OnAbductorBeaconChosenBuiMsg(Entity<AbductorHumanObservationConsoleComponent> ent, ref AbductorBeaconChosenBuiMsg args)
    {
        var consoleTransform = Transform(ent);
        var beacon = _entityManager.GetEntity(args.Beacon.NetEnt);

        if (_xformSys.GetGrid((beacon, Transform(beacon))) is not { } gridUid)
            return;

        var gridTransform = Transform(gridUid);

        if (!_xformSys.InRange((ent, consoleTransform), (gridUid, gridTransform), ent.Comp.MinStationDistance))
            return;

        OnCameraExit(args.Actor);
        if (ent.Comp.RemoteEntityProto != null)
        {
            var eye = SpawnAtPosition(ent.Comp.RemoteEntityProto, Transform(beacon).Coordinates);
            ent.Comp.RemoteEntity = GetNetEntity(eye);

            if (TryComp<HandsComponent>(args.Actor, out var handsComponent))
            {
                foreach (var hand in _hands.EnumerateHands((args.Actor, handsComponent)))
                {
                    if (!_hands.TryGetHeldItem((args.Actor, handsComponent), hand, out var held))
                        continue;

                    if (HasComp<UnremoveableComponent>(held))
                        continue;

                    _hands.DoDrop((args.Actor, handsComponent), hand);
                }

                if (_virtualItem.TrySpawnVirtualItemInHand(ent.Owner, args.Actor, out var virtItem1))
                {
                    EnsureComp<UnremoveableComponent>(virtItem1.Value);
                }

                if (_virtualItem.TrySpawnVirtualItemInHand(ent.Owner, args.Actor, out var virtItem2))
                {
                    EnsureComp<UnremoveableComponent>(virtItem2.Value);
                }
            }

            var visibility = EnsureComp<VisibilityComponent>(eye);

            Dirty(ent);

            if (TryComp(args.Actor, out EyeComponent? eyeComp))
            {
                _eye.SetVisibilityMask(args.Actor, eyeComp.VisibilityMask | (int) VisibilityFlags.Abductor, eyeComp);
                _eye.SetTarget(args.Actor, eye, eyeComp);
                _eye.SetDrawFov(args.Actor, false);
                _eye.SetRotation(args.Actor, Angle.Zero, eyeComp);
                if (!HasComp<StationAiOverlayComponent>(args.Actor))
                    AddComp(args.Actor, new StationAiOverlayComponent { AllowCrossGrid = true });
                if (!TryComp(eye, out RemoteEyeSourceContainerComponent? remoteEyeSourceContainerComponent))
                {
                    remoteEyeSourceContainerComponent = new RemoteEyeSourceContainerComponent { Actor = args.Actor };
                    AddComp(eye, remoteEyeSourceContainerComponent);
                }
                else
                    remoteEyeSourceContainerComponent.Actor = args.Actor;
                Dirty(eye, remoteEyeSourceContainerComponent);
                Dirty(args.Actor, eyeComp);
            }

            AddActions(args);

            _mover.SetRelay(args.Actor, eye);
        }
    }

    private void OnCameraExit(EntityUid actor)
    {
        if (TryComp<RelayInputMoverComponent>(actor, out var comp)
            && TryComp<AbductorScientistComponent>(actor, out var abductorComp))
        {
            var relay = comp.RelayEntity;
            RemComp(actor, comp);

            if (abductorComp.Console != null)
                _virtualItem.DeleteInHandsMatching(actor, abductorComp.Console.Value);

            if (TryComp(actor, out EyeComponent? eyeComp))
            {
                if (HasComp<StationAiOverlayComponent>(actor))
                    RemComp<StationAiOverlayComponent>(actor);

                _eye.SetVisibilityMask(actor, eyeComp.VisibilityMask ^ (int) VisibilityFlags.Abductor, eyeComp);
                _eye.SetDrawFov(actor, true);
                _eye.SetTarget(actor, null, eyeComp);
            }
            RemoveActions(actor);
            QueueDel(relay);
        }
    }

    private void OnBeforeActivatableUIOpen(Entity<AbductorHumanObservationConsoleComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        if (!TryComp<AbductorScientistComponent>(args.User, out var abductorComp))
            return;

        abductorComp.Console = ent.Owner;
        var stations = _stationSystem.GetStations();
        var result = new Dictionary<NetEntity, StationBeacons>();

        var consoleTransform = Transform(ent);

        foreach (var station in stations)
        {
            if (_stationSystem.GetLargestGrid(station) is not { } grid
                || !TryComp(station, out MetaDataComponent? stationMetaData))
                continue;

            var xform = Transform(grid);
            var isEnabled = _xformSys.InRange((ent, consoleTransform), (grid, xform), ent.Comp.MinStationDistance);

            if (!_entityManager.TryGetComponent<NavMapComponent>(grid, out var navMap))
                continue;

            var netEnt = GetNetEntity(station);

            result.Add(netEnt, new StationBeacons
            {
                Name = stationMetaData.EntityName,
                StationId = netEnt,
                Beacons = [.. navMap.Beacons.Values],
                IsEnabled = isEnabled,
            });
        }

        _uiSystem.SetUiState(ent.Owner, AbductorCameraConsoleUIKey.Key, new AbductorCameraConsoleBuiState() { Stations = result });
    }

    private void OnActivatableUIOpenAttempt(Entity<AbductorHumanObservationConsoleComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!HasComp<AbductorScientistComponent>(args.User))
            args.Cancel();
    }

}
