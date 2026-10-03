using Robust.Shared.Serialization;

namespace Content.Shared.BombDefusal;

[NetSerializable, Serializable]
public sealed class BombScannerUiState : BoundUserInterfaceState
{
    public BombRuleSet? RuleSet;

    public BombScannerUiState(BombRuleSet? ruleSet)
    {
        RuleSet = ruleSet;
    }
}
