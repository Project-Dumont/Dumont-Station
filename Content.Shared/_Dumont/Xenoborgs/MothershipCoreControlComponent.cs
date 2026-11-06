// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.Xenoborgs;

/// <summary>
/// Lets a mothership core pilot xenoborgs that have nobody inside them.
/// </summary>
[RegisterComponent]
public sealed partial class MothershipCoreControlComponent : Component
{
    /// <summary>
    /// The xenoborg the core is piloting right now.
    /// </summary>
    [DataField]
    public EntityUid? Controlled;

    /// <summary>
    /// Action given to the piloted xenoborg to send the core back to its body.
    /// </summary>
    [DataField]
    public EntProtoId ReturnAction = "ActionReturnToMothershipCore";
}
