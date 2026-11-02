using Content.Server.GameTicking.Rules;

namespace Content.Server._Dumont.StationTraits;

public sealed partial class ArrivalsSpawnRuleSystem : GameRuleSystem<ArrivalsSpawnRuleComponent>
{
    public bool IsActive()
    {
        var query = QueryActiveRules();
        return query.MoveNext(out _, out _, out _);
    }
}
