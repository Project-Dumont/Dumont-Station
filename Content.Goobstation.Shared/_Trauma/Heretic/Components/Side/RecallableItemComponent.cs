// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Actions.Components;
using Content.Shared.Whitelist;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.Side;

[RegisterComponent, NetworkedComponent]
public sealed partial class RecallableItemComponent : Component
{
    [DataField(required: true)]
    public EntProtoId<ActionComponent> ActionId;

    [DataField]
    public EntityUid? Action;

    [DataField]
    public EntityUid? User;

    [DataField]
    public EntityWhitelist? UserWhitelist;

    [DataField]
    public EntityWhitelist? UserBlacklist;

    [DataField]
    public bool WhitelistCheckMind;
}
