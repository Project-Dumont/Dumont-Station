using Content.Server.Chat.Systems;
using Content.Server.Salvage.Expeditions;
using Content.Server.Shuttles.Events;
using Content.Server.Stack;
using Content.Shared._FarHorizons.Salvage;
using Content.Shared._FarHorizons.Salvage.Components;
using Content.Shared.Chat;
using Content.Shared.Salvage.Expeditions;
using Robust.Shared.Prototypes;

namespace Content.Server._FarHorizons.Salvage;

public sealed class SalvageMissionObjectiveSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StackSystem _stack = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FTLStartedEvent>(OnFTLStarted);
        SubscribeLocalEvent<FTLCompletedEvent>(OnFTLCompleted);
    }

    private void OnFTLStarted(ref FTLStartedEvent ev)
    {
        SalvageExpeditionComponent? exped = null;
        if (TryComp<SalvageExpeditionComponent>(ev.FromMapUid, out exped))
        {
            var handler = GetHandler(exped.Objective);
            if (handler != null && handler.Initialized)
                handler.Exit(ev.Entity);
        } else if (TryComp<SalvageExpeditionComponent>(_transform.GetMap(ev.TargetCoordinates), out exped))
        {
            var handler = GetHandler(exped.Objective);
            if (handler != null && handler.Initialized)
                handler.BeforeFTLToMap(ev.Entity);
        }
    }

    private void OnFTLCompleted(ref FTLCompletedEvent ev)
    {
        if (TryComp<SalvageExpeditionComponent>(ev.MapUid, out var exped))
        {
            var handler = GetHandler(exped.Objective);
            if (handler != null && handler.Initialized)
                handler.AFterFTLToMap(ev.Entity);
            return;
        }
        
        if (GetExpedConsole(ev.Entity) is Entity<SalvageExpeditionConsoleComponent> console &&
            TryComp<SalvageMissionRewardComponent>(console, out var reward))
            ProcessReward((console.Owner, reward));
    }

    private BaseSalvageMissionObjectiveHandler? GetHandler(ProtoId<SalvageMissionObjectivePrototype> objectiveId)
    {
        var objective = _protoMan.Index(objectiveId);
        if (objective.HandlerId != null && 
            _protoMan.Index<SalvageMissionObjectiveHandlerPrototype>(objective.HandlerId) is SalvageMissionObjectiveHandlerPrototype handlerProto &&
            handlerProto.Handler != null)
            return handlerProto.Handler;
        return null;
    }

    private void ProcessReward(Entity<SalvageMissionRewardComponent> ent)
    {
        var objective = _protoMan.Index<SalvageMissionObjectivePrototype>(ent.Comp.parentObjective);

        var totalCash = (int)(ent.Comp.TotalReward * ent.Comp.CashMultiplier);

        if(ent.Comp.TotalReward > 0 && _transform.TryGetMapOrGridCoordinates(ent, out var pos))
        {
            _stack.SpawnMultipleAtPosition(objective.RewardProto, ent.Comp.TotalReward, pos.Value);
            _stack.SpawnMultipleAtPosition(objective.CashProto, totalCash, pos.Value);
        }
        EntityManager.RemoveComponent<SalvageMissionRewardComponent>(ent);

        _chat.TrySendInGameICMessage(
            ent, 
            Loc.GetString(ent.Comp.MissionCompleted ? 
                objective.CompletionText : 
                objective.FailText, 
            ("bonus", ent.Comp.Bonuses), ("maxBonus", ent.Comp.MaxBonuses), ("totalReward", ent.Comp.TotalReward), ("totalCash", totalCash)),
            InGameICChatType.Speak, ChatTransmitRange.GhostRangeLimit, false); // Since apparently errors on listeners can prevent rest of the event from running, moving chat message to the end
    }

    public Entity<SalvageExpeditionConsoleComponent>? GetExpedConsole(EntityUid shuttle)
    {
        var enumerator = Transform(shuttle).ChildEnumerator;
        while(enumerator.MoveNext(out var uid))
            if(TryComp<SalvageExpeditionConsoleComponent>(uid, out var comp))
                return (uid, comp);
        return null;
    }
}