// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.Triage;

/// <summary>
/// Aparelho que marca a triagem. Segure na mão e clique no paciente para abrir a janela.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class TriageTaggerComponent : Component
{
    /// <summary>
    /// Prioridades que o aparelho oferece, na ordem em que aparecem na janela.
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<TriageIconPrototype>> Levels = new();

    /// <summary>
    /// Quem pode ser marcado.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Paciente do último clique, que é quem a janela aberta vai marcar.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public NetEntity? Target;
}
