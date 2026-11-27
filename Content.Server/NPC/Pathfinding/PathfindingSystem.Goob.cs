using Content.Goobstation.Common.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server.NPC.Pathfinding;

public sealed partial class PathfindingSystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private bool _disabled;

    private void InitializeGoob()
    {
        Subs.CVar(_cfg, GoobCVars.DisablePathfinding, x => _disabled = x, true);
    }
}
