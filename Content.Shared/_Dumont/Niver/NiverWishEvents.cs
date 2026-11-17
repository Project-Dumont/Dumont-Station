using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Niver;

[Serializable, NetSerializable]
public sealed class NiverWishRequestEvent(TimeSpan duration) : EntityEventArgs
{
    public readonly TimeSpan Duration = duration;
}

[Serializable, NetSerializable]
public sealed class NiverWishResponseEvent(bool wished) : EntityEventArgs
{
    public readonly bool Wished = wished;
}
