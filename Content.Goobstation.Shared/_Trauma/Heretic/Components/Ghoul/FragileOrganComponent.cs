// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Medical.Shared.Body;

/// <summary>
/// Gibs organ on removal
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FragileOrganComponent : Component;
