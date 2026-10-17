using System.Globalization;
using System.Text.Json;
using Content.Shared._Dumont.Election;

namespace Content.Server._Dumont.Election;

public sealed record TseCandidate(string Number, long Votes, float Percent, ElectionOutcome Outcome);

public sealed record ElectionResult(List<TseCandidate> Candidates, float Sections, string LastUpdate)
{
    public ElectionPhase Phase
    {
        get
        {
            if (Candidates.Exists(c => c.Outcome == ElectionOutcome.Elected))
                return ElectionPhase.Elected;

            if (Candidates.Exists(c => c.Outcome == ElectionOutcome.SecondRound))
                return ElectionPhase.SecondRound;

            return Sections > 0f || Candidates.Exists(c => c.Votes > 0)
                ? ElectionPhase.Counting
                : ElectionPhase.Waiting;
        }
    }

    public static ElectionResult Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var candidates = new List<TseCandidate>();

        foreach (var agr in root.GetProperty("carg")[0].GetProperty("agr").EnumerateArray())
        {
            foreach (var par in agr.GetProperty("par").EnumerateArray())
            {
                foreach (var cand in par.GetProperty("cand").EnumerateArray())
                {
                    var status = cand.GetProperty("st").GetString() ?? string.Empty;
                    var outcome = ElectionOutcome.None;
                    if (status.StartsWith("Eleito", StringComparison.OrdinalIgnoreCase))
                        outcome = ElectionOutcome.Elected;
                    else if (status.StartsWith('2'))
                        outcome = ElectionOutcome.SecondRound;

                    candidates.Add(new TseCandidate(
                        cand.GetProperty("n").GetString() ?? string.Empty,
                        long.Parse(cand.GetProperty("vap").GetString() ?? "0", CultureInfo.InvariantCulture),
                        Number(cand.GetProperty("pvapn")),
                        outcome));
                }
            }
        }

        var time = root.GetProperty("hg").GetString() ?? string.Empty;
        if (time.Length >= 5)
            time = time[..5];

        return new ElectionResult(candidates, Number(root.GetProperty("s").GetProperty("pstn")), time);
    }

    // the TSE writes decimals with a comma
    private static float Number(JsonElement element)
    {
        var text = (element.GetString() ?? "0").Replace(',', '.');
        return float.Parse(text, CultureInfo.InvariantCulture);
    }
}
