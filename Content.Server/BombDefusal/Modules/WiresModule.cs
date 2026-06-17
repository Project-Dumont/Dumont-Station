using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

/// <summary>
/// "Simple Wires" module.
/// 3-6 colored wires; the defuser must cut exactly the correct one.
/// Rules are dynamically randomized per bomb.
/// </summary>
public sealed class WiresModule : BombModule
{
    public List<WireColor> WireColors = new();
    public HashSet<int> CutWires = new();

    /// <summary>
    /// The index of the correct wire to cut.
    /// </summary>
    public int CorrectWireIndex;

    // Generated rules stored on the module
    public List<WiresRule> Rules3 = new();
    public List<WiresRule> Rules4 = new();
    public List<WiresRule> Rules5 = new();
    public List<WiresRule> Rules6 = new();

    public WiresModule()
    {
        Type = BombModuleType.Wires;
    }

    /// <summary>
    /// Generate a random Wires module with randomized rules.
    /// </summary>
    public static WiresModule Generate(IRobustRandom random, string serialNumber)
    {
        var module = new WiresModule();
        var wireCount = random.Next(3, 7); // 3 to 6 wires
        var colors = Enum.GetValues<WireColor>();

        for (var i = 0; i < wireCount; i++)
        {
            module.WireColors.Add(random.Pick(colors));
        }

        // Generate rules for all wire counts (3-6)
        module.Rules3 = GenerateRandomRulesForCount(random, 3);
        module.Rules4 = GenerateRandomRulesForCount(random, 4);
        module.Rules5 = GenerateRandomRulesForCount(random, 5);
        module.Rules6 = GenerateRandomRulesForCount(random, 6);

        // Find the rules for the generated wire count
        var rules = module.GetRulesForCount(wireCount);
        module.CorrectWireIndex = module.EvaluateWiresRules(rules, module.WireColors, serialNumber);

        return module;
    }

    public List<WiresRule> GetRulesForCount(int count)
    {
        return count switch
        {
            3 => Rules3,
            4 => Rules4,
            5 => Rules5,
            6 => Rules6,
            _ => Rules3
        };
    }

    public int EvaluateWiresRules(List<WiresRule> rules, List<WireColor> wires, string serialNumber)
    {
        var hasVowel = serialNumber.Any(c => "AEIOUaeiou".Contains(c));
        var lastDigitOdd = serialNumber.Length > 0 &&
                           char.IsDigit(serialNumber[^1]) &&
                           (serialNumber[^1] - '0') % 2 != 0;

        foreach (var rule in rules)
        {
            var match = rule.ConditionType switch
            {
                "Always" => true,
                "NoColor" => !wires.Contains(rule.Color),
                "LastColor" => wires[^1] == rule.Color,
                "MoreThanOneColor" => wires.Count(w => w == rule.Color) > 1,
                "SerialOdd" => lastDigitOdd,
                "SerialEven" => !lastDigitOdd,
                "SerialVowel" => hasVowel,
                "SerialNoVowel" => !hasVowel,
                _ => false
            };

            if (match)
            {
                int index = 0;
                switch (rule.ResultType)
                {
                    case "Index":
                        index = rule.ResultIndex;
                        break;
                    case "LastColor":
                        var lastIdx = wires.LastIndexOf(rule.ResultColor);
                        index = lastIdx >= 0 ? lastIdx : 0;
                        break;
                    case "FirstColor":
                        var firstIdx = wires.IndexOf(rule.ResultColor);
                        index = firstIdx >= 0 ? firstIdx : 0;
                        break;
                }
                return Math.Clamp(index, 0, wires.Count - 1);
            }
        }

        return 0;
    }

    private static List<WiresRule> GenerateRandomRulesForCount(IRobustRandom random, int wireCount)
    {
        var rules = new List<WiresRule>();
        var colors = Enum.GetValues<WireColor>();

        // Generate 3 conditional rules and 1 fallback (Always) rule
        for (int i = 0; i < 3; i++)
        {
            var rule = new WiresRule();
            rule.ConditionType = random.Pick(new[] { "NoColor", "LastColor", "MoreThanOneColor", "SerialOdd", "SerialEven", "SerialVowel", "SerialNoVowel" });
            rule.Color = random.Pick(colors);
            rule.ResultType = random.Pick(new[] { "Index", "LastColor", "FirstColor" });
            rule.ResultIndex = random.Next(0, wireCount);
            rule.ResultColor = random.Pick(colors);

            var condText = rule.ConditionType switch
            {
                "NoColor" => $"there are no {rule.Color.ToString().ToUpper()} wires",
                "LastColor" => $"the last wire is {rule.Color.ToString().ToUpper()}",
                "MoreThanOneColor" => $"there is more than one {rule.Color.ToString().ToUpper()} wire",
                "SerialOdd" => "the last digit of the serial number is odd",
                "SerialEven" => "the last digit of the serial number is even",
                "SerialVowel" => "the serial number contains a vowel",
                "SerialNoVowel" => "the serial number does not contain a vowel",
                _ => ""
            };

            var resText = rule.ResultType switch
            {
                "Index" => $"cut the {GetOrdinal(rule.ResultIndex + 1)} wire",
                "LastColor" => $"cut the last {rule.ResultColor.ToString().ToUpper()} wire",
                "FirstColor" => $"cut the first {rule.ResultColor.ToString().ToUpper()} wire",
                _ => ""
            };

            rule.RuleText = $"If {condText}, {resText}.";
            rules.Add(rule);
        }

        var fallback = new WiresRule();
        fallback.ConditionType = "Always";
        fallback.ResultType = "Index";
        fallback.ResultIndex = random.Next(0, wireCount);
        fallback.RuleText = $"Otherwise, cut the {GetOrdinal(fallback.ResultIndex + 1)} wire.";
        rules.Add(fallback);

        return rules;
    }

    private static string GetOrdinal(int val)
    {
        return val switch
        {
            1 => "first",
            2 => "second",
            3 => "third",
            4 => "fourth",
            5 => "fifth",
            6 => "sixth",
            _ => $"{val}th"
        };
    }

    public override BombDefusalModuleState GetVisibleState()
    {
        return new WiresModuleState
        {
            IsSolved = IsSolved,
            WireColors = new List<WireColor>(WireColors),
            CutWires = new HashSet<int>(CutWires),
        };
    }

    public override bool ValidateAction(BombModuleAction action)
    {
        if (IsSolved)
            return true;

        if (action is not CutWireAction cutWire)
            return false;

        if (cutWire.WireIndex < 0 || cutWire.WireIndex >= WireColors.Count)
            return false;

        if (CutWires.Contains(cutWire.WireIndex))
            return true; // no-op

        CutWires.Add(cutWire.WireIndex);

        if (cutWire.WireIndex == CorrectWireIndex)
        {
            IsSolved = true;
            return true;
        }

        return false;
    }
}

public sealed class WiresRule
{
    public string ConditionType = string.Empty;
    public WireColor Color;
    public string ResultType = string.Empty;
    public int ResultIndex;
    public WireColor ResultColor;
    public string RuleText = string.Empty;
}
