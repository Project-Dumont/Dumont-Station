using Content.Shared.Radio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.MalfAi.Events;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class RadioKeyUnlockedEvent : EntityEventArgs
{
    [DataField(required: true)]
    public List<ProtoId<RadioChannelPrototype>> Channels = new();

    public RadioKeyUnlockedEvent(List<ProtoId<RadioChannelPrototype>> channels)
    {
        Channels = channels;
    }
};