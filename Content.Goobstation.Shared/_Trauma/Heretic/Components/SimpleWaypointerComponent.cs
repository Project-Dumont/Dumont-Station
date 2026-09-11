// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Trauma.Shared.Waypointer.Components;

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components;

/// <summary>
/// "Clientside" version of <see cref="ActiveWaypointerComponent"/>
/// Server doesn't change this in any way but waypointer overlay still process it
/// Useful when you don't need any actions or pvs overrides
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SimpleWaypointerComponent : ActiveWaypointerComponent;
