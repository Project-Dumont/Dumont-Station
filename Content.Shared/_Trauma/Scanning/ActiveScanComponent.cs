// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.Scanning;

/// <summary>
/// Added while a scanalyzer is scanning an entity.
/// Also applies scan visual effects on client
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class ActiveScanComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan ScanEndTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan ScanStartTime;

    [DataField, AutoNetworkedField]
    public EntityUid ScannedObject;
}
