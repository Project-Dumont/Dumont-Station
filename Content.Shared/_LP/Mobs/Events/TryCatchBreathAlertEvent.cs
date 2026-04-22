using Content.Shared.Alert;
using Robust.Shared.Prototypes;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._LP.Mobs.Events;

public sealed partial class TryCatchBreathAlertEvent : BaseAlertEvent
{
    public TryCatchBreathAlertEvent(EntityUid user, ProtoId<AlertPrototype> alertId)
        : base(user, alertId)
    {
    }
}

[Serializable, NetSerializable]
public sealed partial class TryCatchBreathDoAfterEvent : SimpleDoAfterEvent;
