using Content.Shared._CMU14.Xenomorphs;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphHiveObjectiveComponent : Component
{
    [DataField] public bool Contact;
    [DataField] public bool Hijack;
}

[RegisterComponent]
public sealed partial class XenomorphCommsHijackComponent : Component;

[Serializable, NetSerializable]
public sealed partial class XenomorphCommsHijackDoAfterEvent : SimpleDoAfterEvent;
