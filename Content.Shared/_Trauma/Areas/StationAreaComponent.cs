// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.Areas;

/// <summary>
/// Marker component for areas belonging to the station.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StationAreaComponent : Component;
