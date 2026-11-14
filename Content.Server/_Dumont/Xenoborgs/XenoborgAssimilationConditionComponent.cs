// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Dumont.Xenoborgs;

[RegisterComponent, Access(typeof(XenoborgAssimilationConditionSystem))]
public sealed partial class XenoborgAssimilationConditionComponent : Component
{
    [DataField]
    public float AssimilationWeight = 0.5f;
}
