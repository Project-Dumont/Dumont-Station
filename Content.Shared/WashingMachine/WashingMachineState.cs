using Robust.Shared.Serialization;

namespace Content.Shared.WashingMachine;

[Serializable, NetSerializable]
public enum WashingMachineState : byte
{
    Idle,
    Washing,
    Broken
}
