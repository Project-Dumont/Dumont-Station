// Dumont start
using Robust.Shared.Map;
using Robust.Shared.Serialization;
// Dumont end

namespace Content.Trauma.Common.Heretic;
// Dumont end

[Serializable, NetSerializable]
public sealed class ButtonTagPressedEvent(string id, NetEntity user, NetCoordinates coords) : EntityEventArgs
{
    public NetEntity User = user;

    public NetCoordinates Coords = coords;

    public string Id = id;
}

[ByRefEvent]
public record struct HereticCheckEvent(EntityUid Uid, bool Result = false);
