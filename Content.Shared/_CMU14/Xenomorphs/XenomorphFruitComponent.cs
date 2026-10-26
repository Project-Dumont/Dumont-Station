using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CMU14.Xenomorphs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenomorphFruitComponent : Component
{
    [DataField, AutoNetworkedField] public XenomorphFruitState State;
    [DataField, AutoNetworkedField] public XenomorphFruitKind Kind;
    [DataField] public TimeSpan GrowTime = TimeSpan.FromSeconds(15);
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan GrowAt;
    [DataField] public EntityUid? Planter;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphFruitPlanterComponent : Component
{
    [DataField, AutoNetworkedField] public int Maximum = 3;
    [DataField, AutoNetworkedField] public XenomorphFruitKind? Choice = XenomorphFruitKind.Lesser;
    [DataField, AutoNetworkedField] public List<XenomorphFruitKind> Allowed = new() { XenomorphFruitKind.Lesser };
    [DataField] public int PlasmaCost = 50;
    [DataField] public float HarvestMultiplier = 1;
    [DataField] public float FeedingMultiplier = 1;
    public readonly List<EntityUid> Fruit = new();
}

[Serializable, NetSerializable]
public enum XenomorphFruitState : byte { Growing, Grown, Item, Eaten }

[Serializable, NetSerializable]
public enum XenomorphFruitVisuals : byte { State, Base }

public sealed partial class XenomorphPlantFruitActionEvent : InstantActionEvent;
public sealed partial class XenomorphHealerPlasmaActionEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphFruitHarvestDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphFruitConsumeDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class XenomorphHealerPlasmaDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public enum XenomorphFruitKind : byte { Lesser, Greater, Plasma, Speed, Spore, Unstable }

[Serializable, NetSerializable]
public enum XenomorphFruitUiKey : byte { Choose }

public sealed partial class XenomorphChooseFruitActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed class XenomorphChooseFruitMessage(XenomorphFruitKind kind) : BoundUserInterfaceMessage
{
    public XenomorphFruitKind Kind = kind;
}

[Serializable, NetSerializable]
public sealed class XenomorphFruitUiState(int count, int maximum) : BoundUserInterfaceState
{
    public int Count = count;
    public int Maximum = maximum;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class XenomorphFruitBuffComponent : Component
{
    [DataField, AutoNetworkedField] public float SpeedBonus;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField] public TimeSpan SpeedUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan SporeUntil;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextHeal;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan NextPlasma;
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField] public TimeSpan ShieldDecayAt;
    [DataField] public int HealTicks;
    [DataField] public int HealPerTick;
    [DataField] public int PlasmaTicks;
    [DataField] public float Shield;
    [DataField] public float SporeReduction;
    public EntityUid? ShieldEffect;
    public readonly Dictionary<EntityUid, TimeSpan> ActionDelays = new();
}
