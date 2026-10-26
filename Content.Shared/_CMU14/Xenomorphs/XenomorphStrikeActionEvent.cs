using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Damage;

namespace Content.Shared._CMU14.Xenomorphs;

public sealed partial class XenomorphStrikeActionEvent : EntityTargetActionEvent
{
    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public float ThrowDistance;

    [DataField]
    public float ThrowSpeed = 10;

    [DataField]
    public TimeSpan Knockdown;

    [DataField]
    public TimeSpan Slow;
}

public sealed partial class XenomorphHeadbuttActionEvent : EntityTargetActionEvent;

public sealed partial class XenomorphTailSweepActionEvent : WorldTargetActionEvent;
