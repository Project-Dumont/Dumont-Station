// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

using System.Linq;
using Content.Server.Store.Components;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Console;
// Dumont end

namespace Content.Server.Store.Systems;

public sealed partial class StoreSystem
{
    [Dependency] private IConsoleHost _consoleHost = default!;

    public void InitializeCommand()
    {
        _consoleHost.RegisterCommand("addcurrency", "Adds currency to the specified store", "addcurrency <uid> <currency prototype> <amount>",
            AddCurrencyCommand,
            AddCurrencyCommandCompletions);
    }

    [AdminCommand(AdminFlags.Fun)]
    private void AddCurrencyCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 3)
        {
            shell.WriteError("Argument length must be 3");
            return;
        }

        if (!NetEntity.TryParse(args[0], out var uidNet) || !TryGetEntity(uidNet, out var uid) || !float.TryParse(args[2], out var id))
        {
            return;
        }

        var currency = args[1];
        if (!ProtoMan.HasIndex<CurrencyPrototype>(currency))
        {
            shell.WriteError($"Unknown currency {currency}");
            return;
        }

        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        TryAddCurrency(new() { { currency, id } }, uid.Value, store);
    }

    private CompletionResult AddCurrencyCommandCompletions(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var query = EntityQueryEnumerator<StoreComponent>();
            var allStores = new List<string>();
            while (query.MoveNext(out var storeuid, out _))
            {
                allStores.Add(storeuid.ToString());
            }
            return CompletionResult.FromHintOptions(allStores, "<uid>");
        }

        if (args.Length == 2 && NetEntity.TryParse(args[0], out var uidNet) && TryGetEntity(uidNet, out var uid))
        {
            if (TryComp<StoreComponent>(uid, out var store))
                return CompletionResult.FromHintOptions(store.CurrencyWhitelist.Select(p => p.ToString()), "<currency prototype>");
        }

        return CompletionResult.Empty;
    }
}
