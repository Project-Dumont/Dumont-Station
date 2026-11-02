// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Trauma.Body.Organ;

/// <summary>
/// Marks an organ as kidneys so surgeries can pick them out.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class KidneysComponent : Component;
