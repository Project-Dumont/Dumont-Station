// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Trauma.Server.Station;
using Content.Trauma.Shared.Station; // Dumont
using Robust.Shared.Console;

namespace Content.Trauma.Server.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed partial class ForceStationTraitCommand : IConsoleCommand
{
    [Dependency] private IEntityManager _entMan = default!;
    private StationTraitsSystem? _traits;

    public string Command => "forcestationtrait";
    // Dumont changes start
    public string Description => Loc.GetString("cmd-forcestationtrait-desc");
    public string Help => Loc.GetString("cmd-forcestationtrait-help");
    // Dumont end

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine(Loc.GetString("shell-wrong-arguments-number")); // Dumont
            return;
        }

        _traits ??= _entMan.System<StationTraitsSystem>();

        var id = args[0];
        // Dumont changes start
        shell.WriteLine(_traits.ForceTrait(id)
            ? Loc.GetString("cmd-forcestationtrait-success", ("id", id))
            : Loc.GetString("cmd-forcestationtrait-unknown", ("id", id)));
        // Dumont end
    }

    // Dumont changes start
    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length == 1
            ? CompletionResult.FromHintOptions(CompletionHelper.PrototypeIDs<StationTraitPrototype>(), Loc.GetString("cmd-forcestationtrait-hint"))
            : CompletionResult.Empty;
    }
    // Dumont end
}
