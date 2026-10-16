// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Shared.Humanoid;

namespace Content.Trauma.Server.Heretic.Systems;

[RegisterComponent]
public sealed partial class EnvyDisguiseStateComponent : Component
{
    [DataField]
    public EntityUid? RevertAction;

    [DataField]
    public string? OriginalName;

    [DataField]
    public HumanoidAppearanceComponent? OriginalAppearance;
}
// Dumont end
