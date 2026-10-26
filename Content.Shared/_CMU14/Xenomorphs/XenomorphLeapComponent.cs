using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenomorphLeapComponent : Component
{
    [DataField] public TimeSpan ObstacleStun;
    public bool CheckingMove;
    [DataField] public int PlasmaCost;
    [DataField] public SoundSpecifier? Sound;

    [DataField]
    public float Range = 6;

    [DataField]
    public float Speed = 20;

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan MoveDelay = TimeSpan.FromSeconds(0.7);

    [DataField]
    public bool KnockdownRequiresStealth;

    [DataField]
    public bool UnrootOnMelee;

    [DataField, AutoNetworkedField]
    public bool Leaping;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan RootedUntil;
}

public sealed partial class XenomorphLeapActionEvent : WorldTargetActionEvent;

[ByRefEvent]
public readonly record struct XenomorphLeapHitEvent(EntityUid Target);
