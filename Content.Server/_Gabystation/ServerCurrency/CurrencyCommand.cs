// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Sara Aldrete's Top Guy <malchanceux@protonmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2026 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2026 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Gabystation.ServerCurrency.Managers;
using Content.Server._Gabystation.ServerCurrency.UI;
using Content.Server.Administration;
using Content.Server.EUI;
using Content.Shared._Gabystation.ServerCurrency.Prototypes;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._Gabystation.ServerCurrency
{
    [AnyCommand]
    public sealed class CurrencyUiCommand : IConsoleCommand
    {
        public string Command => "balanceui";

        public string Description => Loc.GetString("server-currency-command-open-balanceui");

        public string Help => $"{Command}";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            if (player == null)
            {
                shell.WriteLine("This does not work from the server console.");
                return;
            }

            var eui = IoCManager.Resolve<EuiManager>();
            var ui = new CurrencyEui();
            eui.OpenEui(ui, player);
        }
    }

    [AdminCommand(AdminFlags.Host)]
    public sealed class CurrencyStoreRotationCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntitySystemManager _entitySystems = default!;

        public string Command => "balance:doRotation";

        public string Description => Loc.GetString("server-currency-command-desc-store-rotation");

        public string Help => $"{Command}";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var sys = _entitySystems.GetEntitySystem<ServerCurrencyStoreSystem>();
            sys.DoStoreRotation();
        }
    }

    [AdminCommand(AdminFlags.Fun)]
    public sealed class AddTitleCommand : IConsoleCommand
    {
        [Dependency] private readonly CurrencyStoreManager _store = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;

        public string Command => "balance:addtitle";

        public string Description => Loc.GetString("server-currency-command-desc-add-title");

        public string Help => $"{Command} <User> <TitleListiningPrototype>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var playerManager = IoCManager.Resolve<IPlayerManager>();
            if (!playerManager.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-1"));
                return;
            }

            if (args[1] is not string || !_proto.HasIndex<TitleListingPrototype>(args[1]))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-unknow-prototype"));
                return;
            }

            if (_store.HasTitle(targetPlayer, args[1]))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-has-item"));
                return;
            }
            _store.AddTitle(targetPlayer, args[1]);
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("server-currency-command-completion-1")),
                2 => CompletionResult.FromHintOptions(CompletionHelper.PrototypeIDs<TitleListingPrototype>(), Loc.GetString("server-currency-command-completion-2")),
                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Fun)]
    public sealed class AddGhostSkinCommand : IConsoleCommand
    {
        [Dependency] private readonly CurrencyStoreManager _store = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;

        public string Command => "balance:addskin";

        public string Description => Loc.GetString("server-currency-command-desc-add-ghost");

        public string Help => $"{Command} <User> <GhostSkinListiningPrototype>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var playerManager = IoCManager.Resolve<IPlayerManager>();
            if (!playerManager.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-1"));
                return;
            }

            if (args[1] is not string || !_proto.HasIndex<GhostSkinListingPrototype>(args[1]))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-unknow-prototype"));
                return;
            }

            if (_store.HasGhostSkin(targetPlayer, args[1]))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-has-item"));
                return;
            }
            _store.AddGhostSkin(targetPlayer, args[1]);
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("server-currency-command-completion-1")),
                2 => CompletionResult.FromHintOptions(CompletionHelper.PrototypeIDs<GhostSkinListingPrototype>(), Loc.GetString("server-currency-command-completion-2")),
                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Fun)]
    public sealed class RemoveTitleCommand : IConsoleCommand
    {
        [Dependency] private readonly CurrencyStoreManager _store = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;

        public string Command => "balance:remtitle";

        public string Description => Loc.GetString("server-currency-command-desc-remove-title");

        public string Help => $"{Command} <User> <TitleListingPrototype>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var playerManager = IoCManager.Resolve<IPlayerManager>();
            if (!playerManager.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-1"));
                return;
            }

            if (!_proto.HasIndex<TitleListingPrototype>(args[1]))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-unknow-prototype"));
                return;
            }

            if (!_store.HasTitle(targetPlayer, args[1]))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-does-not-have-item"));
                return;
            }

            _store.RemoveTitle(targetPlayer, args[1]);
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(
                    CompletionHelper.SessionNames(),
                    Loc.GetString("server-currency-command-completion-1")),

                2 => CompletionResult.FromHintOptions(
                    CompletionHelper.PrototypeIDs<TitleListingPrototype>(),
                    Loc.GetString("server-currency-command-completion-2")),

                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Fun)]
    public sealed class RemoveGhostSkinCommand : IConsoleCommand
    {
        [Dependency] private readonly CurrencyStoreManager _store = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;

        public string Command => "balance:remskin";

        public string Description => Loc.GetString("server-currency-command-desc-remove-skin");

        public string Help => $"{Command} <User> <GhostSkinListingPrototype>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
                return;
            }

            var playerManager = IoCManager.Resolve<IPlayerManager>();
            if (!playerManager.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-1"));
                return;
            }

            if (!_proto.HasIndex<GhostSkinListingPrototype>(args[1]))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-unknow-prototype"));
                return;
            }

            if (!_store.HasGhostSkin(targetPlayer, args[1]))
            {
                shell.WriteError(Loc.GetString("server-currency-command-error-does-not-have-item"));
                return;
            }

            _store.RemoveGhostSkin(targetPlayer, args[1]);
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(
                    CompletionHelper.SessionNames(),
                    Loc.GetString("server-currency-command-completion-1")),

                2 => CompletionResult.FromHintOptions(
                    CompletionHelper.PrototypeIDs<GhostSkinListingPrototype>(),
                    Loc.GetString("server-currency-command-completion-2")),

                _ => CompletionResult.Empty
            };
        }
    }
}
