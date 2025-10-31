using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Salvage.Objectives;

public sealed partial class SalvageMissionRecovery : BaseSalvageMissionObjectiveHandler
{
    private readonly List<EntProtoId> _recoveryTargets = ["CrateSalvageMissionObjective"];

    public override void AFterFTLToMap(EntityUid shuttle) => 
        Announce(GetAnnouncement());
    public override void BeforeFTLFromMap(EntityUid shuttle)
    {
        if (GetExpeditionConsole(shuttle) is not EntityUid expedConsole)
            return;
        
        var allTargets = GetAllMarkedEntitiesOnShuttle(shuttle);
        SetRewardComponent(expedConsole, ResolveCompletion(allTargets.Count));
        DeleteWithEffect(allTargets);
    }
    public override void BeforeFTLToMap(EntityUid shuttle){}

    public override void OnMapCreated()
    {
        for (var i = 0; i < GetNumSpawnables(); i++)
        {
            var proto = _recoveryTargets[Rand.Next(_recoveryTargets.Count)];
            if (GetRandomEmptyTileInDungeon() is not EntityCoordinates pos)
                return;

            SpawnAndMarkEntity(proto, pos);
        }
    }

    private int GetNumSpawnables() => 
        Objective.NumTargets.GetValueOrDefault(Difficulty, 0) + Objective.BonusCap;
}