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

namespace Content.Trauma.Shared.Heretic.Components.Side;

[RegisterComponent, NetworkedComponent]
public sealed partial class MirrorMaidComponent : Component
{
    [DataField]
    public DamageSpecifier ExamineDamage = new()
    {
        DamageDict =
        {
            { "Blunt", 15 },
        }
    };

    [DataField]
    public TimeSpan ExamineDelay = TimeSpan.FromSeconds(5);

    [DataField]
    public EntProtoId ExamineStatus = "ExaminedMirrorMaidStatusEffect";

    [DataField]
    public SoundSpecifier? ExamineSound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/ghost2.ogg");
}
