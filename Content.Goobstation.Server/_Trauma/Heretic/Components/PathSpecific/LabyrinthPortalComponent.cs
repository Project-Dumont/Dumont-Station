// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Random;
// Dumont end

namespace Content.Trauma.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class LabyrinthPortalComponent : Component
{
    [DataField]
    public ProtoId<WeightedRandomEntityPrototype> ToSpawn = "LabyrinthPortalSpawnTable";

    [DataField]
    public float SpawnChance = 1f;

    [DataField]
    public float MinSpawnChance = 0.1f;

    [DataField]
    public float ChanceReduction = 0.1f;

    [DataField]
    public bool Paused;

    [DataField]
    public EntityUid? HereticMind;

    [DataField]
    public List<EntityUid> SpawnedMobs = new();

    [DataField]
    public int MaxMobs = 20;
}
