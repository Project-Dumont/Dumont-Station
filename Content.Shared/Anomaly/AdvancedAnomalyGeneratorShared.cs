// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Anomaly.Prototypes;
using Content.Shared.Materials;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Anomaly;

[Serializable, NetSerializable]
public enum AdvancedAnomalyGeneratorUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class AdvancedAnomalyGeneratorUserInterfaceState(
    List<ProtoId<AdvancedAnomalyGenerationPrototype>> entries,
    ProtoId<MaterialPrototype> material,
    int materialAmount,
    int researchPoints,
    bool canUse,
    TimeSpan cooldownEnd,
    int range,
    NetEntity? grid,
    Vector2i machineTile) : BoundUserInterfaceState
{
    public readonly List<ProtoId<AdvancedAnomalyGenerationPrototype>> Entries = entries;
    public readonly ProtoId<MaterialPrototype> Material = material;
    public readonly int MaterialAmount = materialAmount;
    public readonly int ResearchPoints = researchPoints;
    public readonly bool CanUse = canUse;
    public readonly TimeSpan CooldownEnd = cooldownEnd;
    public readonly int Range = range;
    public readonly NetEntity? Grid = grid;
    public readonly Vector2i MachineTile = machineTile;
}

[Serializable, NetSerializable]
public sealed class AdvancedAnomalyGeneratorGenerateMessage(ProtoId<AdvancedAnomalyGenerationPrototype> entry, Vector2i tile)
    : BoundUserInterfaceMessage
{
    public readonly ProtoId<AdvancedAnomalyGenerationPrototype> Entry = entry;
    public readonly Vector2i Tile = tile;
}

// Same values for the server and UI so the range and cooldown dont get out of sync
// sooo Keeps the server and UI from arguing about the numbers
public static class AdvancedAnomalyGeneratorRules
{
    public static bool InRange(Vector2i origin, Vector2i tile, int range) => (tile - origin).LengthSquared <= range * range;

    public static string FormatCooldown(TimeSpan remaining) => remaining.ToString(@"m\:ss");
}
