using Robust.Shared.Configuration;

namespace Content.Shared._Dumont.CCVar;

[CVarDefs]
public sealed partial class DumontCVars
{
    public static readonly CVarDef<bool> ElectionEnabled =
        CVarDef.Create("election.enabled", true, CVar.SERVERONLY);

    public static readonly CVarDef<string> ElectionUrl =
        CVarDef.Create("election.url",
            "https://resultados.tse.jus.br/oficial/ele2026/6257/dados/br/br-c0001-e006257-u.json",
            CVar.SERVERONLY);

    /// <summary>
    /// seconds between requests to the TSE
    /// </summary>
    public static readonly CVarDef<float> ElectionInterval =
        CVarDef.Create("election.interval", 30f, CVar.SERVERONLY);

    /// <summary>
    /// seconds into the round before candidates get picked and the screens go live
    /// </summary>
    public static readonly CVarDef<float> ElectionDelay =
        CVarDef.Create("election.delay", 900f, CVar.SERVERONLY);

    /// <summary>
    /// seconds into the round before the drop pod with the screen arrives
    /// </summary>
    public static readonly CVarDef<float> ElectionDelivery =
        CVarDef.Create("election.delivery", 300f, CVar.SERVERONLY);

    /// <summary>
    /// how many candidate slots to open while the TSE has not sent the list
    /// </summary>
    public static readonly CVarDef<int> ElectionCandidates =
        CVarDef.Create("election.candidates", 13, CVar.SERVERONLY);

    /// <summary>
    /// fakes a whole count in a few minutes, without asking the TSE
    /// </summary>
    public static readonly CVarDef<bool> ElectionTest =
        CVarDef.Create("election.test", false, CVar.SERVERONLY);

    /// <summary>
    /// seconds the fake count takes to go from 0 to 100%
    /// </summary>
    public static readonly CVarDef<float> ElectionTestDuration =
        CVarDef.Create("election.test_duration", 60f, CVar.SERVERONLY);
}
