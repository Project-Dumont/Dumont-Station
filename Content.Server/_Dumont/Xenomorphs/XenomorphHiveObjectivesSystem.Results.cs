using Content.Server.GameTicking;
using Content.Server._White.GameTicking.Rules.Components;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.GameTicking.Components;

namespace Content.Server._Dumont.Xenomorphs;

public sealed partial class XenomorphHiveObjectivesSystem
{
    private bool _hadXenomorphs;

    private void InitializeResults()
    {
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnStandaloneRoundEndText);
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
        args.AddLine(Loc.GetString($"xenomorphs-{result.ToString().ToLowerInvariant()}"));
    }
}
