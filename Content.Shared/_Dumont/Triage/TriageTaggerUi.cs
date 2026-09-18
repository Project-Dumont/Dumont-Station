// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Triage;

[Serializable, NetSerializable]
public enum TriageTaggerUiKey : byte
{
    Key,
}

/// <summary>
/// Prioridade escolhida na janela. Nulo tira a marcação do paciente.
/// </summary>
[Serializable, NetSerializable]
public sealed class TriageTaggerSetMessage : BoundUserInterfaceMessage
{
    public readonly ProtoId<TriageIconPrototype>? Level;

    public TriageTaggerSetMessage(ProtoId<TriageIconPrototype>? level)
    {
        Level = level;
    }
}
