using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared.Genetics;

public sealed partial class MindCommunicationActionEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed class MindCommunicationMenuOpenedEvent : EntityEventArgs
{
    public NetEntity Uid { get; }

    public MindCommunicationMenuOpenedEvent(NetEntity uid)
    {
        Uid = uid;
    }
}

[Serializable, NetSerializable]
public sealed class MindCommunicationTargetSelectedEvent : EntityEventArgs
{
    public NetEntity Sender { get; }
    public NetEntity Target { get; }

    public MindCommunicationTargetSelectedEvent(NetEntity sender, NetEntity target)
    {
        Sender = sender;
        Target = target;
    }
}
