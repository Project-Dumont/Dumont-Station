// SPDX-FileCopyrightText: 2025 Crono209ggg <crono209gg@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Cocoon
{
    [RegisterComponent]
    public sealed partial class CocoonComponent : Component
    {
        public string? OldAccent;

        public EntityUid? Victim;

        [DataField("damagePassthrough")]
        public float DamagePassthrough = 0.5f;

    }
}
