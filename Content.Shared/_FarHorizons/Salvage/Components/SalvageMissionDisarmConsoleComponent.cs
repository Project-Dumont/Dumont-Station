// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Salvage.Components;

[RegisterComponent]
public sealed partial class SalvageMissionDisarmConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)] public bool Enabled = false;
    [ViewVariables(VVAccess.ReadOnly)] public bool Armed = false;
    [ViewVariables(VVAccess.ReadOnly)] public string Code = string.Empty;

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
public sealed class SalvageMissionDisarmSubmitCodeMessage : BoundUserInterfaceMessage
{
    public string Code { get; }

    public SalvageMissionDisarmSubmitCodeMessage(string code)
    {
        Code = code;
    }
}