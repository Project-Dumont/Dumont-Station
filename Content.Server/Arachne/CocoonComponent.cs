// SPDX-FileCopyrightText: 2025 Crono209ggg <crono209gg@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Arachne
{
    [RegisterComponent]
    public sealed partial class CocoonComponent : Component
    {
        public bool WasReplacementAccent = false;

        public string OldAccent = "";

        [DataField("damagePassthrough")]
        public float DamagePassthrough = 0.5f;
    }
}
