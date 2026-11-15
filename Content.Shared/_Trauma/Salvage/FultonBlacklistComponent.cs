// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Trauma.Salvage;

/// <summary>
/// Component that prevents fultons from being used on this entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FultonBlacklistComponent : Component;
