// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared._Dumont.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Robust.Server.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.Niver;

public sealed partial class NiverRuleSystem : GameRuleSystem<NiverRuleComponent>
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private AudioSystem _audio = default!;

    private static readonly EntProtoId RuleId = "NiverRule";

    private bool _enabled;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        Subs.CVar(_cfg, DumontCVars.NiverEnabled, value => _enabled = value, true);
    }

    // runs every round fire fire fire
    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        if (!_enabled || ev.New != GameRunLevel.InRound)
            return;

        GameTicker.StartGameRule(RuleId);
    }

    protected override void Started(EntityUid uid, NiverRuleComponent comp, GameRuleComponent rule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, rule, args);

        var now = Timing.CurTime;
        comp.GreetTime = now + comp.GreetDelay;
        comp.SongTime = now + RobustRandom.Next(comp.MinSongDelay, comp.MaxSongDelay);
    }

    protected override void ActiveTick(EntityUid uid, NiverRuleComponent comp, GameRuleComponent rule, float frameTime)
    {
        base.ActiveTick(uid, comp, rule, frameTime);

        var now = Timing.CurTime;
        if (comp.GreetTime is { } greet && now >= greet)
        {
            comp.GreetTime = null;
            _chat.DispatchGlobalAnnouncement(Loc.GetString(comp.GreetAnnouncement), colorOverride: Color.Gold);
        }

        if (comp.SongTime is { } song && now >= song)
        {
            comp.SongTime = null;
            _chat.DispatchGlobalAnnouncement(Loc.GetString(comp.SongAnnouncement), colorOverride: Color.Gold);

            var inGame = Filter.Empty().AddWhere(GameTicker.UserHasJoinedGame);
            _audio.PlayGlobal(comp.Song, inGame, true);
        }

        if (comp.GreetTime == null && comp.SongTime == null)
            GameTicker.EndGameRule(uid, rule);
    }
}
