// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Shared.Actions;

namespace Content.Trauma.Shared.Heretic.Components.Side;

public sealed partial class DisguiseRevertEvent : InstantActionEvent
{
    [DataField]
    public bool RaiseRenameEvents;
}
// Dumont end
