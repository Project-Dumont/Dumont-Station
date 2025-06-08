using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Server._Gabystation.Economy;
using Content.Server.GameTicking;
using System.Linq;

namespace Content.Server._Gabystation.Economy.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class GetBankCurrencyCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "getcurrency";

    public string Description => "Get an NanoBank currency";

    public string Help => $"{Command} <accountId>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var economyMan = _entityManager.System<EconomyManagerSystem>();
        if (!economyMan.TryGetBalance(int.Parse(args[0]), out var balance))
            shell.WriteError("Unknow bank account!");

        shell.WriteLine($"{args[0]} balance is: {balance}");
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var economyMan = _entityManager.System<EconomyManagerSystem>();

            var options = economyMan.GetLinkedAccounts()
                .Select(entry =>
                {
                    var (accountId, uid, account) = entry;
                    var job = account.JobId ?? "Unknown";
                    return new CompletionOption(accountId.ToString(),
                        $"{accountId} | {_entityManager.GetComponent<MetaDataComponent>(uid).EntityName}");
                })
                .ToArray();

            return CompletionResult.FromOptions(options);
        }

        return CompletionResult.Empty;
    }


}
