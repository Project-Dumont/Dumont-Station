// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text;
using Content.Server.Chat.Systems;
using Content.Shared.Paper;
using Content.Trauma.Common.CCVar;
using Content.Trauma.Shared.Station;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Trauma.Server.Station;

/// <summary>
/// Creates the station report and sends it to all comms consoles on the station.
/// </summary>
public sealed partial class StationTraitReportSystem : EntitySystem // Dumont
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private PaperSystem _paper = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private StationTraitsSystem _traits = default!;

    private StringBuilder _sb = new();
    private int _years;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationTraitReportComponent, MapInitEvent>(OnMapInit);

        Subs.CVar(_cfg, TraumaCVars.YearOffset, y => _years = y, true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StationTraitReportComponent>();
        while (query.MoveNext(out var station, out var comp))
        {
            if (_timing.CurTime < comp.NextReport)
                continue;

            RemCompDeferred(station, comp);

            var text = CreateReport(station);
            var proto = comp.ReportProto;
            var consoles = EntityQueryEnumerator<StationTraitReportTargetComponent>();
            while (consoles.MoveNext(out var uid, out _))
            {
                SpawnReport(uid, proto, text);
            }

            // TODO: custom greenshift/threat level announcement
            // Dumont changes start
            _chat.DispatchStationAnnouncement(station,
                Loc.GetString("station-traits-report-announcement"),
                Loc.GetString("station-traits-report-sender"));
            // Dumont end
        }
    }

    private void OnMapInit(Entity<StationTraitReportComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextReport = _timing.CurTime + ent.Comp.ReportDelay;
    }

    /// <summary>
    /// Generate the station report text.
    /// </summary>
    public string CreateReport(EntityUid station)
    {
        _sb.Clear();
        var date = DateTime.UtcNow.AddYears(_years).ToString(Loc.GetString("station-traits-report-date-format")); // Dumont
        // Dumont changes start
        _sb.AppendLine(Loc.GetString("station-traits-report-title", ("date", date)));
        _sb.AppendLine();

        // TODO: actual dynamic gamemode reports lol
        _sb.AppendLine(Loc.GetString("station-traits-report-advisory"));
        _sb.AppendLine();
        // Dumont end

        // TODO: station goals

        _traits.AppendReport(_sb, station);

        _sb.AppendLine();
        _sb.AppendLine(Loc.GetString("station-traits-report-footer", ("station", Name(station)))); // Dumont

        return _sb.ToString();
    }

    /// <summary>
    /// Spawn a copy of the report for a console.
    /// </summary>
    public void SpawnReport(EntityUid uid, EntProtoId proto, string text)
    {
        var coords = Transform(uid).Coordinates;
        var report = SpawnAtPosition(proto, coords);
        _paper.SetContent(report, text);
    }
}
