using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Dumont.Election;

// temporary, only here to test the election screen without a full server
[AdminCommand(AdminFlags.Debug)]
public sealed partial class ElectionAddCrewCommand : IConsoleCommand
{
    [Dependency] private IEntityManager _entMan = default!;

    public string Command => "electionaddcrew";
    public string Description => "Adds fake crew members to the election candidate pool.";
    public string Help => $"{Command} <amount>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1 || !int.TryParse(args[0], out var amount) || amount < 1)
        {
            shell.WriteError(Help);
            return;
        }

        _entMan.System<ElectionSystem>().AddFakeCrew(amount);
        shell.WriteLine($"Added {amount} fake crew members.");
    }
}
