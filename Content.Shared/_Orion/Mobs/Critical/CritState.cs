using Robust.Shared.Serialization;

namespace Content.Shared._Orion.Mobs.Critical;

[Serializable, NetSerializable]
public enum CritState : byte
{
    Normal = 0,
    SoftCrit = 1,
    HardCrit = 2,
    Dead = 3,
}
