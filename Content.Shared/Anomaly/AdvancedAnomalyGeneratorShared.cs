// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Anomaly;

[Serializable, NetSerializable]
public enum AdvancedAnomalyGeneratorUiKey : byte
{
    Key = 50
}

[Serializable, NetSerializable]
public readonly record struct AdvancedAnomalyGeneratorEntryState(
    string Id,
    string Name,
    int ResearchCost,
    int PlasmaCost,
    string Prototype);

[Serializable, NetSerializable]
public sealed class AdvancedAnomalyGeneratorUserInterfaceState(
    List<AdvancedAnomalyGeneratorEntryState> entries,
    int plasmaAmount,
    int plasmaCost,
    int researchPoints,
    string? lastMessage,
    bool canUse,
    int defaultTileX,
    int defaultTileY,
    NetEntity stationGrid) : BoundUserInterfaceState
{
    public readonly List<AdvancedAnomalyGeneratorEntryState> Entries = entries;
    public readonly int PlasmaAmount = plasmaAmount;
    public readonly int PlasmaCost = plasmaCost;
    public readonly int ResearchPoints = researchPoints;
    public readonly string? LastMessage = lastMessage;
    public readonly bool CanUse = canUse;
    public readonly int DefaultTileX = defaultTileX;
    public readonly int DefaultTileY = defaultTileY;
    public readonly NetEntity StationGrid = stationGrid;
}

[Serializable, NetSerializable]
public sealed class AdvancedAnomalyGeneratorGenerateMessage : BoundUserInterfaceMessage
{
    public readonly string EntryId;
    public readonly int TileX;
    public readonly int TileY;

    public AdvancedAnomalyGeneratorGenerateMessage(string entryId, int tileX, int tileY)
    {
        EntryId = entryId;
        TileX = tileX;
        TileY = tileY;
    }
}

[Serializable, NetSerializable]
public enum AdvancedAnomalyGeneratorVisualLayers : byte
{
    Base
}
