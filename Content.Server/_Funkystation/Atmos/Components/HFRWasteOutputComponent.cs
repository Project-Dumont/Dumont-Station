// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 MajorMoth <thepolandbear@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Shared.Containers.ItemSlots;
using Content.Server.Atmos.EntitySystems;
using System.Linq;

namespace Content.Server._Funkystation.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class HFRWasteOutputComponent : Component
    {
        [DataField("coreUid")]
        public EntityUid? CoreUid { get; set; }

        [DataField("fusionStarted")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool FusionStarted;

        [DataField("isActive")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsActive;

        [DataField("cracked")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool Cracked;
    }
}