// SPDX-FileCopyrightText: 2025 Crono209ggg <crono209gg@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared.Vampiric
{
    [Serializable, NetSerializable]
    public sealed partial class BloodSuckDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
