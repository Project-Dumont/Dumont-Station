using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Salvage.Components;

[RegisterComponent]
public sealed partial class SalvageMissionDisarmConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)] public bool Enabled = false;
    [ViewVariables(VVAccess.ReadOnly)] public bool Armed = false;
    [ViewVariables(VVAccess.ReadOnly)] public int Code = 0;

    [DataField] public SoundSpecifier? FailSound;
    [DataField] public SoundSpecifier? SuccessSound;
}

[Serializable, NetSerializable]
public enum SalvageMissionDisarmConsoleVisuals : byte
{
    Enabled,
    Armed
}

[Serializable, NetSerializable]
public enum SalvageMissionDisarmConsoleUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class SalvageMissionDisarmSubmitCodeMessage(int code) : BoundUserInterfaceMessage
{
    public int Code = code;
}