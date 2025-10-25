// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 _kote <143940725+FaDeOkno@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Weapons.Ranged;

public sealed class GetRecoilModifiersEvent : EntityEventArgs
{
    public EntityUid Gun;
    public EntityUid User;
    public float Modifier = 1f;
}
