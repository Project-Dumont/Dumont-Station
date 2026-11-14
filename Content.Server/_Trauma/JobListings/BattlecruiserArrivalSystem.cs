// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Trauma.Server.JobListings;

public sealed partial class BattlecruiserArrivalSystem : EntitySystem
{
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private GameTicker _ticker = default!;
    [Dependency] private IGameTiming _timing = default!;

    [SubscribeLocalEvent(after: [typeof(RuleGridsSystem)])]
    private void OnLoaded(Entity<BattlecruiserArrivalComponent> ent, ref RuleLoadedGridsEvent args)
    {
        if (ent.Comp.StartedAt != null || args.Grids.Count == 0)
            return;

        ent.Comp.StartedAt = _timing.CurTime;
        Announce(ent.Comp);
    }

    [SubscribeLocalEvent]
    private void OnEnded(Entity<BattlecruiserArrivalComponent> ent, ref GameRuleEndedEvent args)
    {
        ent.Comp.Music = _audio.Stop(ent.Comp.Music);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BattlecruiserArrivalComponent, ActiveGameRuleComponent>();
        while (query.MoveNext(out _, out var comp, out _))
        {
            Announce(comp);
        }
    }

    private void Announce(BattlecruiserArrivalComponent comp)
    {
        if (comp.StartedAt is not { } start)
            return;

        while (comp.NextAnnouncement < comp.Announcements.Count)
        {
            var announcement = comp.Announcements[comp.NextAnnouncement];
            if (_timing.CurTime < start + announcement.Offset)
                break;

            comp.NextAnnouncement++;
            if (announcement.Message is { } message)
            {
                _chat.DispatchGlobalAnnouncement(Loc.GetString(message),
                    Loc.GetString("battlecruiser-announcement-sender"), playSound: false, colorOverride: Color.Red);
            }

            var players = Filter.Empty().AddWhere(_ticker.UserHasJoinedGame);
            _audio.PlayGlobal(announcement.Sound, players, true);
            if (announcement.Music != null)
            {
                _audio.Stop(comp.Music);
                comp.Music = _audio.PlayGlobal(announcement.Music, players, true)?.Entity;
            }
        }
    }
}
