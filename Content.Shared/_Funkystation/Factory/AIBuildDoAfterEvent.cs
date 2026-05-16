// SPDX-License-Identifier: MIT

using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Factory;

/// <summary>
/// DoAfter event for AI building
/// </summary>
[Serializable, NetSerializable]
public sealed partial class AIBuildDoAfterEvent : DoAfterEvent
{
    public NetCoordinates Location;
    public string Prototype = default!;
    public EntProtoId? RefundAction = null;

    public AIBuildDoAfterEvent() { }

    public AIBuildDoAfterEvent(NetCoordinates location, string prototype, EntProtoId? refundAction)
    {
        Location = location;
        Prototype = prototype;
        RefundAction = refundAction;
    }

    public override DoAfterEvent Clone() => new AIBuildDoAfterEvent(Location, Prototype, RefundAction);
}
