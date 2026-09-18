// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.Triage;

/// <summary>
/// Marcação de triagem grudada no paciente. Só quem usa MedHUD enxerga.
/// Sobrevive à morte e só sai quando alguém tira com o aparelho de triagem.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriageTagComponent : Component
{
    /// <summary>
    /// Prioridade marcada no paciente.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<TriageIconPrototype> Level;
}
