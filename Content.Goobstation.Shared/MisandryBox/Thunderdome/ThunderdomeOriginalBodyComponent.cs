// SPDX-FileCopyrightText: 2026 Goob Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.MisandryBox.Thunderdome;

[RegisterComponent]
public sealed partial class ThunderdomeOriginalBodyComponent : Component
{
    [DataField]
    public NetUserId Owner;
}
