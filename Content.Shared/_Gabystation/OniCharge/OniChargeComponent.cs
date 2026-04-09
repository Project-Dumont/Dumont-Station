// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Gabystation.OniCharge;

[RegisterComponent, NetworkedComponent]
public sealed partial class OniChargeComponent : Component
{
    [DataField]
    public TimeSpan TargetKnockdown = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan WallKnockdown = TimeSpan.FromSeconds(2.5);

    [DataField]
    public TimeSpan ExhaustedKnockdown = TimeSpan.FromSeconds(2);

    [DataField]
    public float FragileObstacleDamage = 80f;

    [DataField]
    public float TargetBluntDamage = 8f;

    [ViewVariables]
    public bool PendingCharge;

    [ViewVariables]
    public bool IsCharging;

    [ViewVariables]
    public HashSet<EntityUid> HitDuringCurrentCharge = new();
}
