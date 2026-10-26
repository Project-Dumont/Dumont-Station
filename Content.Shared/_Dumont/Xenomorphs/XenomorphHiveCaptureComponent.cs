using Content.Shared._CMU14.Xenomorphs;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphHiveClusterComponent : Component
{
    [DataField] public TimeSpan CaptureDelay = TimeSpan.FromMinutes(1);
    public EntityUid? CapturingUser;
    public EntityUid? CaptureTarget;
}

[RegisterComponent]
public sealed partial class XenomorphHivePylonComponent : Component
{
    [DataField] public EntityUid? CapturedTarget;
}

[RegisterComponent]
public sealed partial class XenomorphHiveCaptureTargetComponent : Component;

[RegisterComponent]
public sealed partial class XenomorphHiveCapturedComponent : Component
{
    [DataField] public EntityUid Pylon;
}

[Serializable, NetSerializable]
public sealed partial class XenomorphHiveCaptureDoAfterEvent : SimpleDoAfterEvent
{
    public NetEntity CaptureTarget;
}

// Pylon captures will later be used for the xenomorph objective.
[ByRefEvent]
public readonly record struct XenomorphHivePylonCapturedEvent(EntityUid Target);
