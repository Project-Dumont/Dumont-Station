// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Server.Player;
using Robust.Shared.Utility;
using Content.Server._White.GameTicking.Rules.Components;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.GameTicking.Components;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphHiveObjectivesSystem
{
    [Dependency] private IPlayerManager _resultPlayers = default!;
    [Dependency] private AntagSelectionSystem _resultAntags = default!;

    private bool _hadXenomorphs;
    private readonly Dictionary<EntityUid, string> _roundEndXenomorphs = new();

    private void InitializeResults()
    {
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnStandaloneRoundEndText);
        SubscribeLocalEvent<XenomorphComponent, MindAddedMessage>(OnResultMindAdded);
    }

    private void OnResultMindAdded(Entity<XenomorphComponent> ent, ref MindAddedMessage args)
    {
        _hadXenomorphs = true;
        _roundEndXenomorphs[args.Mind.Owner] = Name(ent);
    }

    public void AppendRoundEndReport(WinType result, RoundEndTextAppendEvent args)
    {
        var xenos = AllEntityQuery<XenomorphComponent, MindContainerComponent>();
        while (xenos.MoveNext(out var uid, out _, out var container))
        {
            if (container.Mind is { } mind)
                _roundEndXenomorphs[mind] = Name(uid);
        }

        var key = result.ToString().ToLowerInvariant();
        args.AddLine(Loc.GetString($"xenomorphs-{key}"));
        if (result != WinType.Neutral)
            args.AddLine(Loc.GetString($"xenomorphs-summary-{key}"));
        args.AddLine(Loc.GetString("xenomorphs-roundend-count", ("count", _roundEndXenomorphs.Count)));
        args.AddLine(Loc.GetString("xenomorphs-roundend-list-start"));

        foreach (var (mind, name) in _roundEndXenomorphs.OrderBy(entry => entry.Value, StringComparer.OrdinalIgnoreCase))
        {
            var escapedName = FormattedMessage.EscapeText(name);
            if (TryComp<MindComponent>(mind, out var component)
                && component.OriginalOwnerUserId is { } userId
                && _resultPlayers.TryGetPlayerData(userId, out var data))
            {
                var user = _resultAntags.GetRoundEndUsername(mind, data.UserName);
                args.AddLine(Loc.GetString("xenomorphs-roundend-list-name-user",
                    ("name", escapedName), ("user", FormattedMessage.EscapeText(user))));
            }
            else
                args.AddLine(Loc.GetString("xenomorphs-roundend-list-name", ("name", escapedName)));
        }
    }

    private void OnStandaloneRoundEndText(RoundEndTextAppendEvent args)
    {
        if (!_hadXenomorphs && !PylonEstablished && !CommsCorrupted && !_invasion.CommsLocked) return;
        // The normal event writes its own result. Admin-spawned xenos still need a report.
        var rules = AllEntityQuery<XenomorphsRuleComponent, GameRuleComponent>();
        if (rules.MoveNext(out _, out _)) return;
        var alive = false;
        var xenos = AllEntityQuery<XenomorphComponent>();
        while (xenos.MoveNext(out var uid, out _))
            if (!_mobs.IsDead(uid)) { alive = true; break; }
        var result = CommsCorrupted ? WinType.XenoMajor
            : !alive ? WinType.CrewMajor
            : _invasion.Invaded ? WinType.XenoMinor
            : _invasion.CrewArrived && _invasion.CrewEscaped ? WinType.CrewMinor
            : WinType.Neutral;
        AppendRoundEndReport(result, args);
    }
}
