// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Tag;
using Robust.Shared.Audio;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Cosmos;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CosmicFieldComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Strength;

    [DataField]
    public SoundSpecifier BombDefuseSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    [DataField]
    public LocId BombDefusePopup = "cosmic-field-component-bomb-defused-message";

    [DataField]
    public ProtoId<TagPrototype> IgnoredTag = "GrenadeIgnoreCosmicField";
}
