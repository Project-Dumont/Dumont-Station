// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Books;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Books.Ui;

[UsedImplicitly]
public sealed class CustomBookBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private CustomBookWindow? _menu;

    public CustomBookBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<CustomBookWindow>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not CustomBookUiState cast)
            return;

        _menu?.Populate(cast.Pages);
    }
}
