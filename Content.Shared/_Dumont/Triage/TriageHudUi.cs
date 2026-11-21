// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Dumont.Triage;

/// <summary>
/// Chave da janela de triagem aberta pelo medhud. A janela fica no paciente, igual ao menu de procurado do sechud
/// A mensagem enviada é a mesma do marcador: <see cref="TriageTaggerSetMessage"/>
/// </summary>
[Serializable, NetSerializable]
public enum TriageHudUiKey : byte
{
    Key,
}
