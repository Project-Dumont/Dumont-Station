// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Dumont.Triage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.Overlays;

/// <summary>
/// Deixa enxergar a marcação de triagem em cima dos pacientes e, com alt-click no paciente, mudar a triagem dele
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShowTriageIconsComponent : Component
{
    /// <summary>
    /// Prioridades que o medhud oferece ao mudar a triagem na ordem em que aparecem
    /// </summary>
    [DataField]
    public List<ProtoId<TriageIconPrototype>> Levels = new()
    {
        "TriageIconImmediate",
        "TriageIconVeryUrgent",
        "TriageIconUrgent",
        "TriageIconStandard",
        "TriageIconNonUrgent",
        "TriageIconDeceased",
    };
}
