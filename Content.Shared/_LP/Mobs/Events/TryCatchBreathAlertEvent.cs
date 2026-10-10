using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Shared._LP.Mobs.Events;

public sealed partial class TryCatchBreathAlertEvent : BaseAlertEvent
{
    public TryCatchBreathAlertEvent(EntityUid user, ProtoId<AlertPrototype> alertId) : base(user, alertId)
    {
    }
}
