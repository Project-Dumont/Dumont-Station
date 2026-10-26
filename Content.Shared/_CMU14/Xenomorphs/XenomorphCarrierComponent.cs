using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenomorphCarrierComponent : Component
{
    [DataField, AutoNetworkedField] public int Parasites;
    [DataField, AutoNetworkedField] public int MaxParasites = 16;
    [DataField, AutoNetworkedField] public int ReservedParasites;
    [DataField, AutoNetworkedField] public int Eggs;
    [DataField, AutoNetworkedField] public int MaxEggs = 8;
    [DataField, AutoNetworkedField] public bool CanGenerateEggs;
    [DataField, AutoNetworkedField] public bool GeneratingEggs;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan? NextEgg;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan? NextDrain;
    public EntityUid? ChildRole;
}

[RegisterComponent]
public sealed partial class XenomorphPortableEggComponent : Component;

public sealed partial class XenomorphCarrierParasiteActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphCarrierEggActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphCarrierReserveActionEvent : InstantActionEvent;
public sealed partial class XenomorphGenerateEggsActionEvent : InstantActionEvent;
public sealed partial class XenomorphResinHoleActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public enum XenomorphCarrierUiKey : byte { Reserve }

[Serializable, NetSerializable]
public sealed class XenomorphCarrierReserveMessage(int count) : BoundUserInterfaceMessage
{
    public int Count = count;
}

[Serializable, NetSerializable]
public sealed partial class XenomorphPlantEggDoAfterEvent : SimpleDoAfterEvent
{
    [DataField] public NetCoordinates Coordinates;
}

[Serializable, NetSerializable]
public sealed partial class XenomorphFillHoleDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public enum XenomorphCarrierVisuals : byte { TrapFilled, TrapContent }

[Serializable, NetSerializable]
public enum XenomorphTrapContent : byte { Empty, Parasite, Acid }

[Serializable, NetSerializable]
public sealed partial class XenomorphFillAcidHoleDoAfterEvent : SimpleDoAfterEvent;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphEggSupportComponent : Component
{
    [DataField, AutoNetworkedField] public bool Fragile;
    [DataField, AutoNetworkedField] public bool Sustained;
    public EntityUid? Sustainer;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ExpiresAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ShortExpiresAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan? DecayAt;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextCheck;
}
