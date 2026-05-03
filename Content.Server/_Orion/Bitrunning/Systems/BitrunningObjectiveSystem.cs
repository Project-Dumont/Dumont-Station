using Content.Goobstation.Common.Effects;
using Content.Goobstation.Shared.Fishing.Events;
using Content.Shared._Orion.Bitrunning;
using Content.Shared._Orion.Bitrunning.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Orion.Bitrunning.Systems;

public sealed class BitrunningObjectiveSystem : EntitySystem
{
    [Dependency] private readonly QuantumServerSystem _server = default!;
    [Dependency] private readonly ByteforgeSystem _byteforge = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SparksSystem _sparks = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BitrunningExitMarkerComponent, StartCollideEvent>(OnExitCollide);
        SubscribeLocalEvent<BitrunningObjectivePointComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<BitrunningObjectiveDeliveryPointComponent, StartCollideEvent>(OnDeliveryCollide);
        SubscribeLocalEvent<BitrunningDomainEnemyObjectiveComponent, MobStateChangedEvent>(OnEnemyStateChanged);
        SubscribeLocalEvent<AvatarConnectionComponent, FishCaughtEvent>(OnFishCaught);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var servers = EntityQueryEnumerator<QuantumServerComponent>();
        while (servers.MoveNext(out var serverUid, out var server))
        {
            if (server.State != BitrunningServerState.Running
                || server.ObjectiveCompleted
                || server.ObjectivePoints > 0
                || (server.ObjectiveType != BitrunningObjectiveType.FillStomach
                    && server.ObjectiveType != BitrunningObjectiveType.OverhydrateStomach))
                continue;

            if (_timing.CurTime < server.NextSatiationProgressTime)
                continue;

            foreach (var avatar in server.ActiveConnections)
            {
                if (!IsAvatarMeetingSatiationObjective(avatar, server.ObjectiveType))
                    continue;

                _server.AddObjectiveProgress(serverUid, 1);
                server.NextSatiationProgressTime = _timing.CurTime + TimeSpan.FromSeconds(1);
                break;
            }
        }
    }

    private void OnExitCollide(Entity<BitrunningExitMarkerComponent> ent, ref StartCollideEvent args)
    {
        if (!HasComp<AvatarConnectionComponent>(args.OtherEntity))
            return;

        if (!TryResolveDomainMapUid(ent.Owner, args.OtherEntity, out var mapUid))
            return;

        if (!_server.TryGetServerByDomainMap(mapUid, out _, out _))
            return;

        _server.DisconnectAvatar(args.OtherEntity, false);
    }

    private void OnInteract(Entity<BitrunningObjectivePointComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryResolveDomainMapUid(ent.Owner, args.User, out var mapUid, out var coordinates))
            return;

        if (!_server.TryGetServerByDomainMap(mapUid, out var serverUid, out var server))
            return;

        if (server.ObjectiveType != BitrunningObjectiveType.CollectEncryptedCaches)
            return;

        _server.AddObjectiveProgress(serverUid, ent.Comp.Points);
        _audio.PlayPvs(ent.Comp.PickupSound, coordinates);
        if (ent.Comp.ConsumeOnUse)
            QueueDel(ent.Owner);

        args.Handled = true;
    }

    private void OnDeliveryCollide(Entity<BitrunningObjectiveDeliveryPointComponent> ent, ref StartCollideEvent args)
    {
        if (!TryResolveDomainMapUid(ent.Owner, args.OtherEntity, out var mapUid))
            return;

        if (!_server.TryGetServerByDomainMap(mapUid, out var serverUid, out var server))
            return;

        if (!HasComp<BitrunningObjectiveCargoComponent>(args.OtherEntity))
            return;

        if (HasComp<BitrunningDeliveredObjectiveCargoComponent>(args.OtherEntity))
            return;

        if (!_byteforge.HasLinkedByteforge(serverUid, server))
        {
            if (TryComp<MapComponent>(mapUid, out var mapComp))
                _popup.PopupEntity(Loc.GetString("bitrunning-delivery-byteforge-required"), ent, Filter.BroadcastMap(mapComp.MapId), true, PopupType.LargeCaution);

            return;
        }

        if (!_byteforge.TryDeliverObjectiveCargoToByteforge(serverUid, args.OtherEntity))
            return;

        _sparks.DoSparks(Transform(ent).Coordinates);

        if (server.ObjectiveType == BitrunningObjectiveType.DeliveryCacheCrate)
            _server.AddObjectiveProgress(serverUid, ent.Comp.Points);
    }

    private void OnEnemyStateChanged(Entity<BitrunningDomainEnemyObjectiveComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (!TryResolveDomainMapUid(ent.Owner, null, out var mapUid))
            return;

        if (!_server.TryGetServerByDomainMap(mapUid, out var serverUid, out var server))
            return;

        if (server.ObjectiveType != BitrunningObjectiveType.EliminateEnemies)
            return;

        _server.AddObjectiveProgress(serverUid, ent.Comp.Points);
    }

    private void OnFishCaught(Entity<AvatarConnectionComponent> ent, ref FishCaughtEvent args)
    {
        if (!TryResolveDomainMapUid(ent.Owner, null, out var mapUid))
            return;

        if (!_server.TryGetServerByDomainMap(mapUid, out var serverUid, out var server))
            return;

        if (server.ObjectiveType != BitrunningObjectiveType.CatchFish)
            return;

        _server.AddObjectiveProgress(serverUid, 1);
    }

    private bool TryResolveDomainMapUid(EntityUid primaryUid, EntityUid? fallbackUid, out EntityUid mapUid, out EntityCoordinates coordinates)
    {
        coordinates = default;
        if (TryComp(primaryUid, out TransformComponent? primaryXform) && primaryXform.MapUid is { } primaryMapUid)
        {
            mapUid = primaryMapUid;
            coordinates = primaryXform.Coordinates;
            return true;
        }

        if (fallbackUid != null && TryComp(fallbackUid.Value, out TransformComponent? fallbackXform) && fallbackXform.MapUid is { } fallbackMapUid)
        {
            mapUid = fallbackMapUid;
            coordinates = fallbackXform.Coordinates;
            return true;
        }

        mapUid = default;
        return false;
    }

    private bool TryResolveDomainMapUid(EntityUid primaryUid, EntityUid? fallbackUid, out EntityUid mapUid)
    {
        return TryResolveDomainMapUid(primaryUid, fallbackUid, out mapUid, out _);
    }

    private bool IsAvatarMeetingSatiationObjective(EntityUid avatarUid, BitrunningObjectiveType objectiveType)
    {
        return objectiveType switch
        {
            BitrunningObjectiveType.FillStomach => TryComp<HungerComponent>(avatarUid, out var hunger) &&
                                                   _hunger.GetHungerThreshold(hunger) >= HungerThreshold.Overfed,
            BitrunningObjectiveType.OverhydrateStomach => TryComp<ThirstComponent>(avatarUid, out var thirst) &&
                                                          thirst.CurrentThirstThreshold >= ThirstThreshold.OverHydrated,
            _ => false,
        };
    }
}
