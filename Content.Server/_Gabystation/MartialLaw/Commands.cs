// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Administration;
using Content.Server.AlertLevel;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Console;

namespace Content.Server._Gabystation.MartialLaw
{
    [UsedImplicitly]
    [AdminCommand(AdminFlags.Fun)]
    public sealed class SetMartialLaw : LocalizedCommands
    {
        [Dependency] private readonly IEntitySystemManager _entitySystems = default!;

        public override string Command => "setmartial";
        public override string Description => "Decreta a lei marcial na estação em que sua entidade está.";
        public override string Help => "setmartial";

        /*public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            var options = new string[] {"false"};
            var player = shell.Player;
            if (player?.AttachedEntity != null)
            {
                var stationUid = _entitySystems.GetEntitySystem<StationSystem>().GetOwningStation(player.AttachedEntity.Value);
            }

            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(options,
                    LocalizationManager.GetString("cmd-setalertlevel-hint-1")),
            };
        }*/

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {

            /*var music = true;
            if (args.Length > 1 && !bool.TryParse(args[1], out music))
            {
                shell.WriteLine(LocalizationManager.GetString("shell-argument-must-be-boolean"));
                return;
            }*/

            var player = shell.Player;
            if (player?.AttachedEntity == null)
            {
                shell.WriteLine(LocalizationManager.GetString("shell-only-players-can-run-this-command"));
                return;
            }

            var stationUid = _entitySystems.GetEntitySystem<StationSystem>().GetOwningStation(player.AttachedEntity.Value);
            if (stationUid == null)
            {
                shell.WriteLine(LocalizationManager.GetString("cmd-setalertlevel-invalid-grid"));
                return;
            }

            var level = "martial";
            var levelNames = GetStationLevelNames(stationUid.Value);
            if (!levelNames.Contains(level))
            {
                shell.WriteLine(LocalizationManager.GetString("cmd-setalertlevel-invalid-level"));
                return;
            }

            _entitySystems.GetEntitySystem<AlertLevelSystem>().SetLevel(stationUid.Value, level, true, true, true, true);
            _entitySystems.GetEntitySystem<AlertLevelSystem>().SetLocked(stationUid.Value, true);

            var announce = _entitySystems.GetEntitySystem<ChatSystem>();
            announce.DispatchStationAnnouncement(stationUid.Value,
                                                LocalizationManager.GetString("martiallaw-start-announce"),
                                                LocalizationManager.GetString("comms-console-announcement-title-centcom"),
                                                false, null, Color.Crimson);

            // TODO: Adicionar suporte a dar acesso aos ids de secoffs
        }

        private string[] GetStationLevelNames(EntityUid station)
        {
            var entityManager = IoCManager.Resolve<IEntityManager>();
            if (!entityManager.TryGetComponent<AlertLevelComponent>(station, out var alertLevelComp))
                return new string[] { };

            if (alertLevelComp.AlertLevels == null)
                return new string[] { };

            return alertLevelComp.AlertLevels.Levels.Keys.ToArray();
        }
    }

    [UsedImplicitly]
    [AdminCommand(AdminFlags.Fun)]
    public sealed class ReleaseMartialLaw : LocalizedCommands
    {
        [Dependency] private readonly IEntitySystemManager _entitySystems = default!;

        public override string Command => "releasemartial";
        public override string Description => "Revoga a lei marcial na estação em que sua entidade está.";
        public override string Help => "releasemartial";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {

            var music = true;
            if (args.Length > 1 && !bool.TryParse(args[1], out music))
            {
                shell.WriteLine(LocalizationManager.GetString("shell-argument-must-be-boolean"));
                return;
            }

            var player = shell.Player;
            if (player?.AttachedEntity == null)
            {
                shell.WriteLine(LocalizationManager.GetString("shell-only-players-can-run-this-command"));
                return;
            }

            var stationUid = _entitySystems.GetEntitySystem<StationSystem>().GetOwningStation(player.AttachedEntity.Value);
            if (stationUid == null)
            {
                shell.WriteLine(LocalizationManager.GetString("cmd-setalertlevel-invalid-grid"));
                return;
            }

            var level = "green";
            var levelNames = GetStationLevelNames(stationUid.Value);
            if (!levelNames.Contains(level))
            {
                shell.WriteLine(LocalizationManager.GetString("cmd-setalertlevel-invalid-level"));
                return;
            }

            _entitySystems.GetEntitySystem<AlertLevelSystem>().SetLocked(stationUid.Value, false);
            _entitySystems.GetEntitySystem<AlertLevelSystem>().SetLevel(stationUid.Value, level, false, true, true, true);

            var announce = _entitySystems.GetEntitySystem<ChatSystem>();
            announce.DispatchStationAnnouncement(stationUid.Value,
                                                LocalizationManager.GetString("martiallaw-end-announce"),
                                                LocalizationManager.GetString("comms-console-announcement-title-centcom"),
                                                false, new SoundPathSpecifier("/Audio/Announcements/war.ogg"), Color.Crimson);
        }

        private string[] GetStationLevelNames(EntityUid station)
        {
            var entityManager = IoCManager.Resolve<IEntityManager>();
            if (!entityManager.TryGetComponent<AlertLevelComponent>(station, out var alertLevelComp))
                return new string[] { };

            if (alertLevelComp.AlertLevels == null)
                return new string[] { };

            return alertLevelComp.AlertLevels.Levels.Keys.ToArray();
        }
    }
}
