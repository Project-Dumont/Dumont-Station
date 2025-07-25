// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.StationTeleporter;

[Serializable, NetSerializable]
public enum StationTeleporterConsoleUIKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class StationTeleporterState : BoundUserInterfaceState
{
    public NetEntity? SelectedTeleporter;
    public List<StationTeleporterStatus> Teleporters;
    public StationTeleporterState(List<StationTeleporterStatus> teleporters, NetEntity? selected = null)
    {
        Teleporters = teleporters;
        SelectedTeleporter = selected;
    }
}

[Serializable, NetSerializable]
public sealed class StationTeleporterStatus
{
    public StationTeleporterStatus(NetEntity teleporterUid, NetCoordinates coordinates, NetCoordinates? link, string name, bool powered)
    {
        TeleporterUid = teleporterUid;
        Coordinates = coordinates;
        LinkCoordinates = link;
        Name = name;
        Powered = powered;
    }

    public NetEntity TeleporterUid;
    public NetCoordinates? Coordinates;
    public NetCoordinates? LinkCoordinates;
    public string Name;
    public bool Powered;
}

[Serializable, NetSerializable]
public sealed class StationTeleporterClickMessage : BoundUserInterfaceMessage
{
    public NetEntity? Teleporter;

    /// <summary>
    /// Called when the client clicks on any active Teleporter on the StationTeleporterConsoleComponent
    /// </summary>
    public StationTeleporterClickMessage(NetEntity? teleporter)
    {
        Teleporter = teleporter;
    }
}

[Serializable, NetSerializable]
public enum TeleporterPortalVisuals
{
    Color,
}
