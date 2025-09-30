// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Server._Gabystation.Economy;
using Content.Server.GameTicking;
using System.Linq;
using Content.Server.Station.Systems;

namespace Content.Server._Gabystation.Economy.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class GetBankCurrencyCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystems = default!;

    public string Command => "bank:get";

    public string Description => "Get an NanoBank currency";

    public string Help => $"{Command} <accountId>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args[0] is null)
            return;

        var player = shell.Player;
        if (player?.AttachedEntity == null)
        {
            shell.WriteLine(Loc.GetString("shell-only-players-can-run-this-command"));
            return;
        }

        var stationUid = _entitySystems.GetEntitySystem<StationSystem>().GetOwningStation(player.AttachedEntity.Value);
        if (stationUid is null)
        {
            shell.WriteLine(Loc.GetString("cmd-setalertlevel-invalid-grid"));
            return;
        }

        if (!_entityManager.TryGetComponent<EconomyManagerComponent>(stationUid.Value, out var economy))
            return;

        var economyMan = _entityManager.System<EconomyManagerSystem>();
        if (!economyMan.TryGetBalance(economy, int.Parse(args[0]), out var balance))
            shell.WriteError("Unknown bank account!");

        shell.WriteLine($"{args[0]} balance is: {balance}");
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var economyMan = _entityManager.System<EconomyManagerSystem>();

            var options = economyMan.GetAllLinkedAccounts()
                .Select(entry =>
                {
                    var (accountId, uid, account) = entry;
                    var job = account.JobId ?? "Unknown";
                    return new CompletionOption(accountId.ToString(),
                        $"{_entityManager.GetComponent<MetaDataComponent>(uid).EntityName}");
                })
                .ToArray();

            return CompletionResult.FromOptions(options);
        }

        return CompletionResult.Empty;
    }


}

[AdminCommand(AdminFlags.Admin)]
public sealed class SetBankCurrencyCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystems = default!;

    public string Command => "bank:set";

    public string Description => "Set an NanoBank currency";

    public string Help => $"{Command} <accountId> <currency>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args[0] is null || args[1] is null)
            return;

        var player = shell.Player;
        if (player?.AttachedEntity == null)
        {
            shell.WriteLine(Loc.GetString("shell-only-players-can-run-this-command"));
            return;
        }

        var stationUid = _entitySystems.GetEntitySystem<StationSystem>().GetOwningStation(player.AttachedEntity.Value);
        if (stationUid is null)
        {
            shell.WriteLine(Loc.GetString("cmd-setalertlevel-invalid-grid"));
            return;
        }

        if (!_entityManager.TryGetComponent<EconomyManagerComponent>(stationUid.Value, out var economy))
            return;

        var economyMan = _entityManager.System<EconomyManagerSystem>();
        if (!economyMan.TryGetData(economy, int.Parse(args[0]), out var data))
        {
            shell.WriteLine("Unknow account!");
            return;
        }
        if (data is null) // I love c# compiler s2
            return;

        data.Balance = int.Parse(args[1]);
        economyMan.SetAccountData(economy, int.Parse(args[0]), data);
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var economyMan = _entityManager.System<EconomyManagerSystem>();

            var options = economyMan.GetAllLinkedAccounts()
                .Select(entry =>
                {
                    var (accountId, uid, account) = entry;
                    var job = account.JobId ?? "Unknown";
                    return new CompletionOption(accountId.ToString(),
                        $"{_entityManager.GetComponent<MetaDataComponent>(uid).EntityName}");
                })
                .ToArray();

            return CompletionResult.FromOptions(options);
        }

        return CompletionResult.Empty;
    }


}

[AdminCommand(AdminFlags.Admin)]
public sealed class DoPaymentCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystems = default!;

    public string Command => "bank:payment";
    public string Description => "Do the payment time";
    public string Help => $"{Command}";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;
        if (player?.AttachedEntity == null)
        {
            shell.WriteLine(Loc.GetString("shell-only-players-can-run-this-command"));
            return;
        }

        var stationUid = _entitySystems.GetEntitySystem<StationSystem>().GetOwningStation(player.AttachedEntity.Value);
        if (stationUid is null)
        {
            shell.WriteLine(Loc.GetString("cmd-setalertlevel-invalid-grid"));
            return;
        }

        var comp = _entityManager.GetComponent<EconomyManagerComponent>(stationUid.Value);
        comp.PaymentCooldownRemaining = 1;
    }
}
