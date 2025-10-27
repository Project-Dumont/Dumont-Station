// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 _kote <143940725+FaDeOkno@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Weapons.Ranged;

/// <summary>
/// Raised on a user when projectiles have been fired from gun.
/// </summary>
public sealed class AmmoShotUserEvent : EntityEventArgs
{
    public EntityUid Gun;
    public List<EntityUid> FiredProjectiles = default!;
}
