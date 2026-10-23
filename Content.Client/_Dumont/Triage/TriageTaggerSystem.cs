// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Dumont.Triage;
using Robust.Shared.GameStates;

namespace Content.Client._Dumont.Triage;

/// <summary>
/// Atualiza a janela aberta quando o médico clica em outro paciente sem fechar a anterior.
/// </summary>
public sealed class TriageTaggerSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriageTaggerComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(Entity<TriageTaggerComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (_ui.TryGetOpenUi<TriageTaggerBoundUserInterface>(ent.Owner, TriageTaggerUiKey.Key, out var bui))
            bui.Update();
    }
}
