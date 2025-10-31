using System.Linq;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

namespace Content.Server._FarHorizons.Salvage.Objectives;

public sealed partial class SalvageMissionCapture : BaseSalvageMissionObjectiveHandler
{
    public override void AFterFTLToMap(EntityUid shuttle) => 
        Announce(GetAnnouncement());
    public override void BeforeFTLFromMap(EntityUid shuttle)
    {
        if (GetExpeditionConsole(shuttle) is not EntityUid expedConsole)
            return;
        
        var allTargets = GetAllMarkedEntitiesOnShuttle(shuttle);
        var aliveTargets = allTargets.Where(p => EntMan.TryGetComponent<MobStateComponent>(p, out var state) && state.CurrentState == MobState.Alive).ToHashSet();
        SetRewardComponent(expedConsole, ResolveCompletion(aliveTargets.Count));
        DeleteWithEffect(aliveTargets);
    }
    public override void BeforeFTLToMap(EntityUid shuttle){}

    public override void OnMapCreated()
    {
        foreach(var mob in GetAllSpawnedMobs())
            MarkEntity(mob);
    }
}