using Content.Shared._Gabystation.ServerCurrency.Prototypes;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.ServerCurrency.Titles;

public sealed class MsgSelectTitle : NetMessage
{
    public ProtoId<TitleListingPrototype>? Proto { get; set; }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Proto = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Proto);
    }
}
