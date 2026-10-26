using Content.Shared._CMU14.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Dumont.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenomorphRoyalComponent : Component
{
    [DataField, AutoNetworkedField] public bool Mature;
    [DataField] public TimeSpan MaturationDelay = TimeSpan.FromMinutes(10);
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan MatureAt;
    public EntityUid? Watched;
    public readonly List<EntityUid> Leaders = new();
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphQueenScreechBlindComponent : Component
{
    [DataField, AutoNetworkedField] public float Radius = 2;
    public TimeSpan Until;
    public bool AddedDeafness;
    public readonly HashSet<EntityUid> ModifiedGuns = new();
}

public sealed partial class XenomorphQueenScreechActionEvent : InstantActionEvent;
public sealed partial class XenomorphQueenSpitActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphQueenGutActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphQueenHealActionEvent : WorldTargetActionEvent;
public sealed partial class XenomorphQueenPlasmaActionEvent : EntityTargetActionEvent;
public sealed partial class XenomorphQueenHiveActionEvent : InstantActionEvent;
public sealed partial class XenomorphQueenMessageActionEvent : InstantActionEvent
{
    [DataField] public string Mode = "word";
}

[Serializable, NetSerializable]
public sealed partial class XenomorphQueenGutDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public enum XenomorphQueenUiKey : byte { Key }

[Serializable, NetSerializable]
public sealed class XenomorphQueenUiState(List<XenomorphQueenMember> members, string mode, bool attached, NetEntity? watched, string pheromones = "") : BoundUserInterfaceState
{
    public List<XenomorphQueenMember> Members = members;
    public string Mode = mode;
    public bool Attached = attached;
    public NetEntity? Watched = watched;
    public string Pheromones = pheromones;
}

[Serializable, NetSerializable]
public sealed class XenomorphQueenMember(NetEntity entity, string name, string caste, bool leader, int tier = 0)
{
    public NetEntity Entity = entity;
    public string Name = name;
    public string Caste = caste;
    public bool Leader = leader;
    public int Tier = tier;
}

[Serializable, NetSerializable]
public sealed class XenomorphQueenUiMessage(string operation, NetEntity? target = null, string text = "") : BoundUserInterfaceMessage
{
    public string Operation = operation;
    public NetEntity? Target = target;
    public string Text = text;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphQueenScreechScatterComponent : Component
{
    [DataField, AutoNetworkedField] public EntityUid Holder;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class XenomorphPheromoneEffectComponent : Component
{
    [DataField, AutoNetworkedField] public float Frenzy;
    [DataField, AutoNetworkedField] public float Warding;
    [DataField, AutoNetworkedField] public float Recovery;
    [DataField, AutoNetworkedField] public string Source = "";
}

[Serializable, NetSerializable]
public sealed class XenomorphQueenAnnouncementEvent(string message) : EntityEventArgs
{
    public string Message = message;
}
