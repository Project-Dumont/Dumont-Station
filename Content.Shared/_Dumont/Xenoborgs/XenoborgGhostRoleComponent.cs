// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Dumont.Xenoborgs;

/// <summary>
/// Opens a ghost role on a xenoborg that has a brain but nobody playing it.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenoborgGhostRoleComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry Components = new();

    /// <summary>
    /// How long a player that ghosted out with the option to return gets before anyone else can take it.
    /// </summary>
    [DataField]
    public TimeSpan ReturnGracePeriod = TimeSpan.FromSeconds(60);

    [DataField]
    public bool Open;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? NextCheck;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? AbandonedAt;
}
