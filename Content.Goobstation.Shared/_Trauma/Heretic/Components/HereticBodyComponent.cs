// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components;

/// <summary>
/// Component added to entities without a mind, previous mind of which was a heretic
/// Used for heretic-heretic sacrifices
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HereticBodyComponent : Component;
