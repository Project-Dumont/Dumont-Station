using Robust.Shared.Serialization;

namespace Content.Shared._DragonStation.FighterProgression;

[Serializable, NetSerializable]
public enum FighterSkillAvailability : byte
{
    Hidden,
    Locked,
    RequirementLocked,
    NextAutoUnlock,
    BranchChoiceAvailable,
    BlockedByBranchChoice,
    Unlocked,
}
