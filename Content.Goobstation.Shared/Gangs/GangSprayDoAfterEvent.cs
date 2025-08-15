using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Gangs;

[Serializable, NetSerializable]
public sealed partial class GangSprayDoAfterEvent : SimpleDoAfterEvent
{
    public NetEntity GangEntity;

    public GangSprayDoAfterEvent(NetEntity gangEntity)
    {
        GangEntity = gangEntity;
    }
}
