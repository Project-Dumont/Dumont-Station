// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Robust.Shared.GameStates;

namespace Content.Shared._Trauma.Medical.Surgery;

/// <summary>
/// Requires that a body part slot does (not) exist on the target part for a surgery to be possible.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryPartSlotConditionComponent : Component
{
    [DataField(required: true)]
    public string Slot = string.Empty;

    [DataField(required: true)]
    public BodyPartType PartType;

    [DataField]
    public BodyPartSymmetry Symmetry = BodyPartSymmetry.None;

    /// <summary>
    /// If true the slot must not exist yet.
    /// </summary>
    [DataField]
    public bool Inverse;
}
