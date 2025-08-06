// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Pinpointer;
using Content.Shared._Gabystation.StationTeleporter.Components;
using Content.Shared.Teleportation.Components;
using Robust.Shared.Map;
using Robust.Shared.Containers;

namespace Content.Shared._Gabystation.StationTeleporter;

public abstract partial class SharedStationTeleporterSystem
{

    public readonly LocId UnknownNameLoc = "teleporter-name-unknown";
    public readonly LocId RdFirstPortalLoc = "teleporter-name-rd-first";
    public readonly LocId RdSecondPortalLoc = "teleporter-name-rd-second";

    // TODO Adicionar caso em que a entidade é deletada;

    private void InitializeUI()
    {
        SubscribeLocalEvent<StationTeleporterConsoleComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<StationTeleporterConsoleComponent, StationTeleporterClickMessage>(OnUIPortalClicked);

        SubscribeLocalEvent<StationTeleporterConsoleComponent, EntRemovedFromContainerMessage>(OnRemove);
        SubscribeLocalEvent<StationTeleporterConsoleComponent, EntInsertedIntoContainerMessage>(OnInsert);
    }

    private void OnUIOpened(Entity<StationTeleporterConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUserInterface(ent);
    }

    private void OnUIPortalClicked(Entity<StationTeleporterConsoleComponent> ent,
        ref StationTeleporterClickMessage args)
    {
        ConsoleInteract(ent, ref args);
        UpdateUserInterface(ent);
    }

    private void OnRemove(Entity<StationTeleporterConsoleComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ChipStorageName)
            return;

        UpdatePortals(ent);
        UpdateUserInterface(ent);
    }

    private void OnInsert(Entity<StationTeleporterConsoleComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.ChipStorageName)
            return;

        UpdatePortals(ent);
        UpdateUserInterface(ent);
    }

    private void UpdatePortals(Entity<StationTeleporterConsoleComponent> ent)
    {
        HashSet<EntityUid> teleporters = new();
        HashSet<EntityUid> handTeleporters = new();

        if (_container.TryGetContainer(ent, ent.Comp.ChipStorageName, out var container))
            foreach (var entity in container.ContainedEntities)
                AddTeleporters(entity, ref teleporters, ref handTeleporters);

        ent.Comp.Teleporters = teleporters;
        ent.Comp.HandTeleporters = handTeleporters;

        void AddTeleporters(EntityUid entity, ref HashSet<EntityUid> teleporters, ref HashSet<EntityUid> handTeleporters)
        {
            if (TryComp<TeleporterChipComponent>(entity, out var chipComp)
                && chipComp.ConnectedTeleporter is { } teleporterUid
                && !Deleted(teleporterUid))
            {
                teleporters.Add(teleporterUid);
                return;
            }

            if (HasComp<HandTeleporterComponent>(entity))
            {
                handTeleporters.Add(entity);
                return;
            }
        }
    }

    private void UpdateUserInterface(Entity<StationTeleporterConsoleComponent> ent)
    {
        if (!_uiSystem.IsUiOpen(ent.Owner, StationTeleporterConsoleUIKey.Key))
            return;

        // The grid must have a NavMapComponent to visualize the map in the UI
        var xform = Transform(ent);

        if (xform.GridUid is not null)
            EnsureComp<NavMapComponent>(xform.GridUid.Value);

        List<StationTeleporterStatus> teleportersData = new();

        foreach (var teleporterUid in ent.Comp.Teleporters)
        {
            _link.GetLink(teleporterUid, out var linkedTeleporter);
            var powered = _power.IsPowered(teleporterUid);

            var teleporterName = LabelQuery.TryComp(teleporterUid, out var label)
                ? label.CurrentLabel ?? Loc.GetString(UnknownNameLoc)
                : Loc.GetString(UnknownNameLoc);

            teleportersData.Add(new(
                GetNetEntity(teleporterUid),
                GetNetEntity(linkedTeleporter),
                Loc.GetString(teleporterName),
                powered
            ));
        }

        foreach (var handTeleporterUid in ent.Comp.HandTeleporters)
        {
            if (!TryComp<HandTeleporterComponent>(handTeleporterUid, out var handTeleporterComp))
                continue;

            if (handTeleporterComp.FirstPortal is not null && EntityManager.EntityExists(handTeleporterComp.FirstPortal))
                AddPortal(handTeleporterComp.FirstPortal.Value, Loc.GetString(RdFirstPortalLoc), ref teleportersData);

            if (handTeleporterComp.SecondPortal is not null && EntityManager.EntityExists(handTeleporterComp.SecondPortal))
                AddPortal(handTeleporterComp.SecondPortal.Value, Loc.GetString(RdSecondPortalLoc), ref teleportersData);
        }

        _uiSystem.SetUiState(ent.Owner,
            StationTeleporterConsoleUIKey.Key,
            new StationTeleporterState(teleportersData, GetNetEntity(ent.Comp.SelectedTeleporter)));

        void AddPortal(EntityUid ent, string name, ref List<StationTeleporterStatus> teleportersData)
        {
            _link.GetLink(ent, out var linkedTeleporter);
            teleportersData.Add(new(GetNetEntity(ent), GetNetEntity(linkedTeleporter), name, true));
        }
    }
}
