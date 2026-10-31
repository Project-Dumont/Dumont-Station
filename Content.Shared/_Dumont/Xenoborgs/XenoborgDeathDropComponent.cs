// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.Xenoborgs;

[RegisterComponent]
public sealed partial class XenoborgDeathDropComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Proto;

    [DataField]
    public bool Dropped;
}
