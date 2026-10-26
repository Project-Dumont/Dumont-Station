using Content.Shared._CMU14.Xenomorphs;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Xenomorphs;

[Serializable, NetSerializable]
public enum XenomorphHiveUiKey : byte { Construction }

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphHiveBuilderComponent : Component
{
    [DataField, AutoNetworkedField] public EntProtoId Choice = "DumontHiveCoreConstructionNode";
    [DataField] public float Range = 1.75f;
    [DataField] public TimeSpan OrderDelay = TimeSpan.FromSeconds(3);
    [DataField] public TimeSpan AddPlasmaDelay = TimeSpan.FromSeconds(3);
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphHiveConstructionNodeComponent : Component
{
    [DataField(required: true), AutoNetworkedField] public FixedPoint2 InitialPlasmaCost;
    [DataField(required: true), AutoNetworkedField] public FixedPoint2 PlasmaCost;
    [DataField, AutoNetworkedField] public FixedPoint2 PlasmaStored;
    [DataField(required: true), AutoNetworkedField] public EntProtoId Spawn;
}

[RegisterComponent]
public sealed partial class XenomorphHiveStructureComponent : Component
{
    [DataField(required: true)] public string Kind = "";
    [DataField] public int Limit = 1;
    [DataField] public bool RequiresCore = true;
    [DataField] public bool RequiresHiveWeeds = true;
    [DataField] public bool RequiresSpace = true;
    [DataField] public FixedPoint2 RepairCost;
    [DataField] public FixedPoint2 RepairStored;
}

[RegisterComponent]
public sealed partial class XenomorphHiveCoreComponent : Component;

[RegisterComponent]
public sealed partial class XenomorphHiveWeedComponent : Component;

public sealed partial class XenomorphOrderHiveConstructionActionEvent : InstantActionEvent;
public sealed partial class XenomorphPlaceHiveConstructionActionEvent : WorldTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphOrderHiveConstructionDoAfterEvent : SimpleDoAfterEvent
{
    public EntProtoId Choice;
    public NetCoordinates Coordinates;
}

[Serializable, NetSerializable]
public sealed partial class XenomorphAddHivePlasmaDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphRepairHiveDoAfterEvent : SimpleDoAfterEvent;
