// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Gabystation.StationTeleporter;
using Content.Shared._Gabystation.StationTeleporter.Components;
using Robust.Shared.Containers;

namespace Content.Server._Gabystation.StationTeleporter.Systems;

public sealed class StationTeleporterSystem : SharedStationTeleporterSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationTeleporterConsoleComponent, MapInitEvent>(OnConsoleInit);

        SubscribeLocalEvent<StationTeleporterConsoleComponent, EntRemovedFromContainerMessage>(OnRemove);
        SubscribeLocalEvent<StationTeleporterConsoleComponent, EntInsertedIntoContainerMessage>(OnInsert);
    }

    private void OnConsoleInit(Entity<StationTeleporterConsoleComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.AutoLinkKey is null || ent.Comp.AutoLinkChipsProto is null)
            return;

        var query = EntityQueryEnumerator<StationTeleporterComponent>();
        while (query.MoveNext(out var teleporterUid, out var teleporter))
        {
            if (teleporter.AutoLinkKey is null || ent.Comp.AutoLinkKey != teleporter.AutoLinkKey)
                continue;

            //Spawn chip inside this console
            var chipEnt = SpawnInContainerOrDrop(ent.Comp.AutoLinkChipsProto, ent, ent.Comp.ChipStorageName);
            if (TryComp<TeleporterChipComponent>(chipEnt, out var chipComp))
            {
                ConnectChipToTeleporter((chipEnt, chipComp), (teleporterUid, teleporter));
            }
        }

        UpdatePortals(ent);
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
}
