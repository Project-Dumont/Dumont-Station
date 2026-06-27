using Robust.Shared.Serialization;

namespace Content.Shared._Orion.Mobs.Critical;

[Serializable, NetSerializable]
public sealed class CritStateChangedMessage : EntityEventArgs
{
    public NetEntity Entity;
    public CritState OldState;
    public CritState NewState;

    public CritStateChangedMessage(NetEntity entity, CritState oldState, CritState newState)
    {
        Entity = entity;
        OldState = oldState;
        NewState = newState;
    }
}
