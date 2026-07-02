using Robust.Shared.Serialization;

namespace Content.Shared._Starlight.MassDriver;

[Serializable, NetSerializable]
public enum MassDriverConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum MassDriverMode : byte
{
    Auto,
    Manual
}

[Serializable, NetSerializable]
public enum MassDriverVisuals : byte
{
    Launching
}

[Serializable, NetSerializable]
public sealed class MassDriverComponentState : ComponentState
{
    public float MaxThrowSpeed;
    public float MaxThrowDistance;
    public float MinThrowSpeed;
    public float MinThrowDistance;
    public float CurrentThrowSpeed;
    public float CurrentThrowDistance;
    public MassDriverMode CurrentMassDriverMode = MassDriverMode.Auto;
    public NetEntity? Console;
    public bool Hacked;
}

[Serializable, NetSerializable]
public sealed class MassDriverUpdateUIMessage(MassDriverComponentState state) : BoundUserInterfaceMessage
{
    public MassDriverComponentState State = state;
}

[Serializable, NetSerializable]
public sealed class MassDriverModeMessage(MassDriverMode mode) : BoundUserInterfaceMessage
{
    public MassDriverMode Mode = mode;
}

[Serializable, NetSerializable]
public sealed class MassDriverLaunchMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class MassDriverThrowSpeedMessage(float speed) : BoundUserInterfaceMessage
{
    public float Speed = speed;
}

[Serializable, NetSerializable]
public sealed class MassDriverThrowDistanceMessage(float distance) : BoundUserInterfaceMessage
{
    public float Distance = distance;
}

[Serializable, NetSerializable]
public enum SecurityWireActionKey : byte
{
    Key,
    Status,
    Pulsed,
    PulseCancel
}
