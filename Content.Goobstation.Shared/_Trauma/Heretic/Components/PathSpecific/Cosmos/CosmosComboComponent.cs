// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Damage;
using Robust.Shared.Audio;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Cosmos;

[RegisterComponent, NetworkedComponent]
public sealed partial class CosmosComboComponent : Component
{
    [DataField]
    public Dictionary<EntityUid, int> HitEntities = new();

    [DataField]
    public float ComboDuration = 3f;

    [DataField]
    public float ComboIncreaseTime = 0.5f;

    [DataField]
    public float MaxComboDuration = 10f;

    [DataField]
    public float ComboTimer = 3f;

    [DataField]
    public int ComboCounter;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/cosmic_energy.ogg");

    [DataField]
    public DamageSpecifier DamageToSecondTargets = new()
    {
        DamageDict =
        {
            { "Heat", 14 },
        },
    };

    [DataField]
    public DamageSpecifier DamageToThirdTargets = new()
    {
        DamageDict =
        {
            { "Heat", 28 },
        },
    };

    [DataField]
    public EntProtoId SecondTargetEffect = "EffectCosmicExplosion";

    [DataField]
    public EntProtoId ThirdTargetEffect = "EffectCosmicDomain";
}
