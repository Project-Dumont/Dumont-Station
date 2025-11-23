// SPDX-FileCopyrightText: 2025 Goob Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Projectiles;

[RegisterComponent]
public sealed partial class ProjectileMissTargetPartChanceComponent : Component
{
    [DataField]
    public List<EntityUid> PerfectHitEntities = new();
}
