using System.Numerics;
using Content.Shared.Nyanotrasen.Abilities.Oni;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Shared._Goobstation.Weapons.Multishot;

namespace Content.Shared.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class JammedGunComponent : Component
{
    [DataField]
    public bool isNotHeldDown = false;
    [DataField]
    public SoundSpecifier? SoundEmpty = new SoundPathSpecifier("/Audio/Weapons/Guns/Empty/empty.ogg");
}

