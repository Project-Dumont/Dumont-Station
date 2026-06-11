// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Anomaly.Prototypes;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Anomaly.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class GeneratingAdvancedAnomalyGeneratorComponent : Component
{
    [DataField("endTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan EndTime = TimeSpan.Zero;

    public EntityUid? AudioStream;

    [DataField]
    public ProtoId<AdvancedAnomalyGenerationPrototype> EntryId = string.Empty;

    [DataField]
    public Vector2i Tile;

    [DataField]
    public EntityUid? User;

    [DataField]
    public int PlasmaConsumed;
}
