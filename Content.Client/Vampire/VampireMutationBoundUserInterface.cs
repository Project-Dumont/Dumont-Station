// SPDX-FileCopyrightText: 2024 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dreykor <arguemeu@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Vampire;
using Content.Shared.Vampire.Components;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
namespace Content.Client.Vampire;

[UsedImplicitly]
public sealed class VampireMutationBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private VampireMutationMenu? _menu;
    public VampireMutationBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }
    protected override void Open()
    {
        base.Open();
        _menu = new VampireMutationMenu();
        _menu.OnClose += Close;
        _menu.OnIdSelected += OnIdSelected;
        _menu.OpenCentered();
    }
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not VampireMutationBoundUserInterfaceState st)
            return;
        _menu?.UpdateState(st.MutationList, st.SelectedMutation);
    }
    private void OnIdSelected(VampireMutationsType selectedId)
    {
        SendMessage(new VampireMutationPrototypeSelectedMessage(selectedId));
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _menu?.Close();
            _menu = null;
        }
    }
}
