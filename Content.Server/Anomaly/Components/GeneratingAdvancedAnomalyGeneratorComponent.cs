// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Anomaly.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Anomaly.Components;

[RegisterComponent, AutoGenerateComponentPause, Access(typeof(AdvancedAnomalyGeneratorSystem))]
public sealed partial class GeneratingAdvancedAnomalyGeneratorComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan EndTime;

    [DataField]
    public ProtoId<AdvancedAnomalyGenerationPrototype> Entry = string.Empty;

    [DataField]
    public Vector2i Tile;

    [DataField]
    public EntityUid? User;

    // Keep track of who paid so the refund goes back to the right server even if this gets relinked later
    [DataField]
    public EntityUid Server;

    public EntityUid? AudioStream;
}
