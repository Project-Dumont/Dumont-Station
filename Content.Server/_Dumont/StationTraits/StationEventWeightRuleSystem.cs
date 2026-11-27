using Content.Server.GameTicking.Rules;

namespace Content.Server._Dumont.StationTraits;

public sealed partial class StationEventWeightRuleSystem : GameRuleSystem<StationEventWeightRuleComponent>
{
    public float GetModifier(string id)
    {
        var modifier = 1f;
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var comp, out _))
        {
            if (comp.Modifiers.TryGetValue(id, out var mod))
                modifier *= mod;
        }

        return modifier;
    }
}
