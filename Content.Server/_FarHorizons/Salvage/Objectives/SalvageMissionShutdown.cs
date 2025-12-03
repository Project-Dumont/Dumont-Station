using System.Linq;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Salvage.Objectives;

public sealed partial class SalvageMissionShutdown : BaseSalvageMissionObjectiveHandler
{
    private readonly List<EntProtoId> _shutdownTargets = ["ServerSalvageMissionObjective"];

    public override void AFterFTLToMap(EntityUid shuttle) => 
        Announce(GetAnnouncement());
    public override void BeforeFTLFromMap(EntityUid shuttle)
    {
        if (GetExpeditionConsole(shuttle) is not EntityUid expedConsole)
            return;
        
        var allTargets = GetAllMarkedEntities();
        var targetsDestroyed = GetNumSpawnables() - allTargets.Count();
        SetRewardComponent(expedConsole, ResolveCompletion(targetsDestroyed));
    }
    public override void BeforeFTLToMap(EntityUid shuttle){} // Override intentionally left empty

    public override void OnMapCreated()
    {
        for (var i = 0; i < GetNumSpawnables(); i++)
        {
            var proto = _shutdownTargets[Rand.Next(_shutdownTargets.Count)];
            if (GetRandomEmptyTileInDungeon() is not EntityCoordinates pos)
                return;

            SpawnAndMarkEntity(proto, pos);
        }
    }

    private int GetNumSpawnables() => 
        Objective.NumTargets.GetValueOrDefault(Difficulty, 0) + Objective.BonusCap;
}