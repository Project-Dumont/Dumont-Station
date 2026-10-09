// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Trauma.Medical.Surgery;

/// <summary>
/// Adds a body part slot to the target part when the step is complete.
/// Requires <see cref="SurgeryPartSlotConditionComponent"/> on
/// the surgery entity in order to specify the slot.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryAddPartSlotStepComponent : Component;
