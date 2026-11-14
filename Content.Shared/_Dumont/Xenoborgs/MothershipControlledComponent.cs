// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Shared._Dumont.Xenoborgs;

/// <summary>
/// Put on a xenoborg while a mothership core is piloting it.
/// </summary>
[RegisterComponent]
public sealed partial class MothershipControlledComponent : Component
{
    [DataField]
    public EntityUid? Core;

    [DataField]
    public EntityUid? ReturnActionEntity;
}

public sealed partial class ReturnToMothershipCoreEvent : InstantActionEvent;
