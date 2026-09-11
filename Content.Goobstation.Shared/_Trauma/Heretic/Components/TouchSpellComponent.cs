// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Whitelist;
using Robust.Shared.Audio;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TouchSpellComponent : Component
{
    [DataField]
    public EntityWhitelist? TargetWhitelist;

    [DataField]
    public EntityWhitelist? TargetBlacklist;

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField]
    public TimeSpan Cooldown;

    [DataField]
    public LocId? Speech;

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public bool CanUseOnSelf;

    [DataField]
    public bool BypassNullrod;
}
