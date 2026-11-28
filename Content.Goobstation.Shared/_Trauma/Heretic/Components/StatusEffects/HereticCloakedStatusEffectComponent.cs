// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Robust.Shared.Audio;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.StatusEffects;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticCloakedStatusEffectComponent : Component
{
    [DataField]
    public bool RequiresFocus = true;

    [DataField]
    public LocId? LoseFocusMessage;

    [DataField]
    public SoundSpecifier? CloakSound = new SoundCollectionSpecifier("Curse");

    [DataField]
    public SoundSpecifier? UncloakSound = new SoundCollectionSpecifier("Curse");
}
