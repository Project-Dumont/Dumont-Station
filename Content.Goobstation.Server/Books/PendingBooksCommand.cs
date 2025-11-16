// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Books;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class PendingBooksCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entities = default!;

    public override string Command => "pending_books";
    public override string Help => "Show the list of pending books awaiting approval";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 0)
        {
            shell.WriteError(LocalizationManager.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (shell.Player == null)
            return;

        var books = _entities.System<CustomBooksSystem>();
        books.OpenPendingBooks(shell.Player);
    }
}
