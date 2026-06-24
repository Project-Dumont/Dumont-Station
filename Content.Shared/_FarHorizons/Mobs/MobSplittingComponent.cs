using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Mobs;

[RegisterComponent, NetworkedComponent]
public sealed partial class MobSplittingComponent : Component
{
    [DataField(required: true)] public Dictionary<EntProtoId, MobSplittingConfig> SplitInto;
    [DataField] public SoundSpecifier? Sound;
    [DataField] public int ThrowRadius = 2;
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class MobSplittingConfig
{
    [DataField] public int Min;
    [DataField] public int Max;
}