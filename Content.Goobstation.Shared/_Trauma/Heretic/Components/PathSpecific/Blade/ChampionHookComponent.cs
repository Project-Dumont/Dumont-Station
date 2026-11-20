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

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Blade;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChampionHookComponent : Component
{
    public override bool SessionSpecific => true;

    [DataField, AutoNetworkedField]
    public EntityUid? HookedMob;

    [DataField, AutoNetworkedField]
    public EntityUid? Weapon;

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2);

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/blood3.ogg");

    [DataField]
    public float OffhandAttackSpeedBuff = 0.35f;

    [DataField]
    public float MovespeedBuff = 0.3f;

    [DataField]
    public DamageSpecifier ExtraDamage = new()
    {
        DamageDict =
        {
            { "Slash", 5 },
        },
    };
}
