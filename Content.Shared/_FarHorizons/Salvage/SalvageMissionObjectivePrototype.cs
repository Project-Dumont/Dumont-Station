using Content.Shared.Procedural;
using Content.Shared.Salvage.Expeditions;
using Content.Shared.Salvage.Expeditions.Modifiers;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Salvage;

[Prototype]
public sealed partial class SalvageMissionObjectivePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField]
    public List<ProtoId<SalvageDifficultyPrototype>>? AllowedDifficulties;
    [DataField]
    public List<ProtoId<SalvageBiomeModPrototype>>? AllowedBiomes;
    [DataField]
    public List<ProtoId<SalvageFactionPrototype>>? AllowedFactions;
    [DataField]
    public List<ProtoId<SalvageDungeonModPrototype>>? AllowedDungeons;
    [DataField]
    public EntProtoId DeleteTargetEffect = "CryoPortal";
    [DataField]
    public SoundSpecifier DeleteTargetSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
    [DataField]
    public LocId CompletionText = "salvage-mission-objective-completed-message";
    [DataField]
    public LocId FailText = "salvage-mission-objective-failed-message";
    [DataField]
    public EntProtoId RewardProto = "SalvageTicket";
    [DataField] public EntProtoId CashProto = "SpaceCash";

    [DataField(required: true)]
    public LocId Name;
    [DataField]
    public Color Color = Color.FromHex("#A88B5E");
    [DataField(required: true)]
    public LocId Description;
    [DataField(required: true)]
    public LocId Announcement;
    [DataField]
    public Dictionary<ProtoId<SalvageDifficultyPrototype>, int> BaseReward = [];
    [DataField]
    public Dictionary<ProtoId<SalvageDifficultyPrototype>, int> NumTargets = [];

    [DataField]
    public int Bonus = 0;
    [DataField]
    public int BonusCap = 0;

    [DataField] public float CashMultiplier;

    [DataField]
    public string? HandlerId;

}