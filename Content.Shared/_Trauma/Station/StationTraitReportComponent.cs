// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Trauma.Shared.Station;

/// <summary>
/// Station component to spawn delayed station reports at comms consoles with <see cref="StationTraitReportTargetComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentPause]
public sealed partial class StationTraitReportComponent : Component // Dumont
{
    /// <summary>
    /// The paper item to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId ReportProto;

    /// <summary>
    /// How long into the round will the report be given.
    /// </summary>
    [DataField]
    public TimeSpan ReportDelay = TimeSpan.FromSeconds(30);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextReport;
}
