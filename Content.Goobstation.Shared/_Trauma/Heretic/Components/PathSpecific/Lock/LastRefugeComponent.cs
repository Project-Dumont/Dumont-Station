// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Components.StatusEffects;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Lock;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LastRefugeComponent : Component
{
    [DataField]
    public float Visibility = 0.3f;

    [DataField]
    public LocId ExamineMessage = "heretic-last-refuge-examine-message";

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(60);

    [DataField]
    public EntProtoId<HereticCloakedStatusEffectComponent> Status = "LastRefugeStatusEffect";

    [DataField, AutoNetworkedField]
    public bool HadStealth;

    [DataField, AutoNetworkedField]
    public bool HadGodmode;

    [DataField, AutoNetworkedField]
    public bool HadSlowdownImmunity;

    [DataField, AutoNetworkedField]
    public bool HadStrippable;
}
