using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ModSuits.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(ModCoreSystem))]
public sealed partial class ModCoreComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool HasBattery;

    [DataField, AutoNetworkedField]
    public float Charge;

    [DataField, AutoNetworkedField]
    public float MaxCharge;
}

[RegisterComponent, NetworkedComponent]
[Access(typeof(ModCoreSystem))]
public sealed partial class ModCoreRefuelableComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Fuel = new();

    [DataField]
    public float ChargePerFuel = 200f;

    [DataField]
    public SoundSpecifier RefuelSound = new SoundPathSpecifier("/Audio/Effects/refill.ogg");
}
