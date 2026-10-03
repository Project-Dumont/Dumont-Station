using System.Numerics;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class JammedGunComponent : Component
{
    [DataField]
    public SoundSpecifier? SoundJammed = new SoundPathSpecifier("/Audio/Weapons/Guns/Empty/empty.ogg");
}
