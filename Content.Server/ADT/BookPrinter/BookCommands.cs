// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.ADT.BookPrinter.Commands;

[AdminCommand(AdminFlags.Server)]
public sealed class DeleteBookCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public override string Command => "deletebook";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 1)
        {
            shell.WriteError("Use: deletebook <ID>");
            shell.WriteError("Exemplo: deletebook 67");
            return;
        }

        if (!int.TryParse(args[0], out var bookId))
        {
            shell.WriteError("O ID do livro deve ser um número!");
            return;
        }

        DeleteBookAsync(shell, bookId);
    }

    private async void DeleteBookAsync(IConsoleShell shell, int bookId)
    {
        try
        {
            var success = await _db.DeleteBookPrinterEntryAsync(bookId);

            if (success)
            {
                shell.WriteLine($"O livro com ID {bookId} removido com sucesso do banco de dados.");

                var bookPrinterSystem = IoCManager.Resolve<IEntitySystemManager>()
                    .GetEntitySystem<BookPrinterSystem>();
                bookPrinterSystem.RefreshBookContent();
            }
            else
            {
                shell.WriteError($"o livro com ID {bookId} não foi encontrado no banco de dados.");
            }
        }
        catch (Exception ex)
        {
            shell.WriteError($"Erro ao remover livro: {ex.Message}");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(["<ID>"], "O ID do livro a ser removido");
        }

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Server)]
public sealed class ListBooksCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;

    public override string Command => "listbooks";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        ListBooksAsync(shell);
    }

    private async void ListBooksAsync(IConsoleShell shell)
    {
        try
        {
            var books = await _db.GetBookPrinterEntriesAsync();

            if (!books.Any())
            {
                shell.WriteLine("O banco de dados de livros está vazio.");
                return;
            }

            shell.WriteLine($"Os livros encontrados no banco de dados foram {books.Count()}");
            shell.WriteLine("=====================================");

            foreach (var book in books.OrderBy(b => b.Id))
            {
                shell.WriteLine($"ID: {book.Id}");
                shell.WriteLine($"Nome: {book.Name}");
                shell.WriteLine($"Descrição: {book.Description}");
                shell.WriteLine($"Tamanho: {book.Content.Length} caracteres");
                shell.WriteLine("-------------------------------------");
            }
        }
        catch (Exception ex)
        {
            shell.WriteError($"Erro ao obter a lista de livros: {ex.Message}");
        }
    }
}
