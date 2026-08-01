// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.Pinpointer;
using Content.Shared._Dumont.Silicons.StationAi;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Mobs.Systems;
using Content.Shared.Silicons.StationAi;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Dumont.Silicons.StationAi;

/// <summary>
/// deixa a IA marcar um ponto pro coletivo silicon
/// o binário já carregava "vai pro cargo", mas nada amarrava a mensagem a um lugar. o ponto
/// nomeia o local pelo beacon mais próximo e deixa uma âncora pra IA voltar
/// limite conhecido: o borg recebe o ponto como mensagem na mente coletiva, não como marcador
/// desenhado no mundo. desenhar só pros silicons pede um overlay de cliente, que é bem mais
/// trabalho que o resto do sistema e ficou de fora de propósito.
/// </summary>
public sealed class StationAiWaypointSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly TransformSystem _xform = default!;

    /// <summary>
    /// canal que o coletivo silicon já divide.
    /// </summary>
    private const string BinaryMind = "Binary";

    private const string MarkerProto = "AiWaypointMarker";

    /// <summary>
    /// ponto velho é pior que ponto nenhum então eles vencem sozinhos
    /// </summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// um marcador por IA. marcar de novo move ele em vez de empilhar
    /// </summary>
    private readonly Dictionary<EntityUid, (EntityUid Marker, string Area, TimeSpan At)> _waypoints = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiHeldComponent, AiPlaceWaypointEvent>(OnPlaceWaypoint);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        foreach (var (_, waypoint) in _waypoints)
        {
            QueueDel(waypoint.Marker);
        }

        _waypoints.Clear();
    }

    private void OnPlaceWaypoint(Entity<StationAiHeldComponent> ent, ref AiPlaceWaypointEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        Clear(ent.Owner);

        var marker = Spawn(MarkerProto, args.Target);
        var area = FormattedMessage.RemoveMarkupPermissive(_navMap.GetNearestBeaconString((marker, null)));

        _waypoints[ent.Owner] = (marker, area, _timing.CurTime);

        Announce(ent.Owner, Loc.GetString("station-ai-waypoint-set", ("area", area)));
    }

    /// <summary>
    /// o ponto vivo da IA, se ela tem um e ele não venceu.
    /// </summary>
    public bool TryGetWaypoint(EntityUid ai, out EntityUid marker, out string area)
    {
        marker = default;
        area = string.Empty;

        if (!_waypoints.TryGetValue(ai, out var waypoint))
            return false;

        if (Deleted(waypoint.Marker) || _timing.CurTime > waypoint.At + Lifetime)
        {
            Clear(ai);
            return false;
        }

        marker = waypoint.Marker;
        area = waypoint.Area;
        return true;
    }

    private void Clear(EntityUid ai)
    {
        if (!_waypoints.Remove(ai, out var waypoint))
            return;

        QueueDel(waypoint.Marker);
    }

    /// <summary>
    /// fala no binário, montado na mão em vez de usar o ChatSystem porque o caminho de envio da
    /// mente coletiva é privado e espera um jogador de verdade falando
    /// </summary>
    private void Announce(EntityUid source, string message)
    {
        if (!_proto.TryIndex<CollectiveMindPrototype>(BinaryMind, out var mind))
            return;

        var clients = Filter.Empty();
        var query = EntityQueryEnumerator<CollectiveMindComponent, ActorComponent>();
        while (query.MoveNext(out var uid, out var collective, out var actor))
        {
            if (_mobState.IsDead(uid))
                continue;

            if (collective.Minds.ContainsKey(mind.ID) || collective.HearAll)
                clients.AddPlayer(actor.PlayerSession);
        }

        if (clients.Count == 0)
            return;

        var wrapped = Loc.GetString("station-ai-waypoint-wrap",
            ("channel", mind.LocalizedName),
            ("message", message));

        _chatManager.ChatMessageToManyFiltered(clients,
            ChatChannel.CollectiveMind,
            message,
            wrapped,
            source,
            false,
            true,
            mind.Color);
    }
}
