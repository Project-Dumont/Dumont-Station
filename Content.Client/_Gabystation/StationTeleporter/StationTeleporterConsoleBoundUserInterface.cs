// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared._Gabystation.StationTeleporter;
using Robust.Client.UserInterface;

namespace Content.Client._Gabystation.StationTeleporter;

public sealed class StationTeleporterConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private StationTeleporterConsoleWindow? _window;

    protected override void Open()
    {
        base.Open();

        EntityUid? gridUid = null;
        var stationName = string.Empty;

        if (EntMan.TryGetComponent<TransformComponent>(Owner, out var xform))
        {
            gridUid = xform.GridUid;

            if (EntMan.TryGetComponent<MetaDataComponent>(gridUid, out var metaData))
            {
                stationName = metaData.EntityName;
            }
        }

        _window = this.CreateWindow<StationTeleporterConsoleWindow>();

        _window.OnTeleporterSelected += teleporter => SendMessage(new StationTeleporterClickMessage(teleporter));
        _window.Set(this, stationName, gridUid);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not StationTeleporterState st)
            return;

        EntMan.TryGetComponent<TransformComponent>(Owner, out var xform);
        _window?.ShowTeleporters(st, Owner, xform?.Coordinates);
    }
}
