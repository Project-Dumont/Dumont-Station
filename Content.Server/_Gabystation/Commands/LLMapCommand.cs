// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2026 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Text;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Map;

namespace Content.Server._Gabystation.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class LLMapCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;

    public override string Command => "llmap";

    // PVS prevents the player from knowing about all maps.
    public override bool RequireServerOrSingleplayer => true;

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var msg = new StringBuilder();

        foreach (var mapId in _mapSystem.GetAllMapIds().OrderBy(id => (int) id))
        {
            if (!_mapSystem.TryGetMap(mapId, out var mapUid))
                continue;

            if (_entManager.Deleted(mapUid))
            {
                msg.AppendFormat("{0}: invalid, ent: {1}\n",
                    mapId,
                    mapUid);
                continue;
            }

            msg.AppendFormat("{0}: {1}, init: {2}, paused: {3}, net_ent: {4}, grids: {5}\n",
                mapId,
                _entManager.GetComponent<MetaDataComponent>(mapUid.Value).EntityName,
                _mapSystem.IsInitialized(mapUid),
                _mapSystem.IsPaused(mapId),
                _entManager.GetNetEntity(mapUid),
                string.Join(",", _map.GetAllGrids(mapId).Select(grid => grid.Owner)));
        }

        shell.WriteLine(msg.ToString());
    }
}