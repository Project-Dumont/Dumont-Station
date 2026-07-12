// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Shared.Destructible;

/// <summary>
/// When this entity is destroyed, destructible thresholds should NOT spawn scraps
/// because the entity is being crushed/covered by the pod.
/// </summary>
[RegisterComponent]
public sealed partial class DroppodSuppressedComponent : Component
{
}
