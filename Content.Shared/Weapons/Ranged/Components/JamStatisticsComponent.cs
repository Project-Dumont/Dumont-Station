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

[RegisterComponent]
public sealed partial class JamStatisticsComponent : Component
{
    // The average amount of shots before a gun jams; EA.
    [DataField]
    int? average;
}
