// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.Buckle;

/// <summary>
/// Whatever has this component can be dragged by other entities
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BuckleableComponent : Component;
