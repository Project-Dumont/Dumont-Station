using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

/// <summary>
/// "Simon Says" module.
/// Colored lights flash in a sequence; the player must press the remapped colors.
/// Color mapping is dynamically randomized per bomb and depends on serial number and strike count.
/// </summary>
public sealed class SimonSaysModule : BombModule
{
    public List<SimonColor> FullSequence = new();
    public int TotalStages;
    public int CurrentStage;
    public int InputProgress;
    public bool SerialHasVowel;

    // Mappings: strikes (0, 1, 2+) -> flashColor -> buttonColor
    public Dictionary<int, Dictionary<SimonColor, SimonColor>> VowelMappings = new();
    public Dictionary<int, Dictionary<SimonColor, SimonColor>> NoVowelMappings = new();

    public SimonSaysModule()
    {
        Type = BombModuleType.SimonSays;
    }

    public static SimonSaysModule Generate(IRobustRandom random, string serialNumber)
    {
        var module = new SimonSaysModule();
        module.TotalStages = random.Next(3, 6); // 3 to 5 stages
        module.SerialHasVowel = serialNumber.Any(c => "AEIOUaeiou".Contains(c));

        // Generate the mappings
        for (int strikes = 0; strikes <= 2; strikes++)
        {
            module.VowelMappings[strikes] = GenerateRandomColorMap(random);
            module.NoVowelMappings[strikes] = GenerateRandomColorMap(random);
        }

        // Generate the full sequence
        var colors = Enum.GetValues<SimonColor>();
        for (var i = 0; i < module.TotalStages; i++)
        {
            module.FullSequence.Add(random.Pick(colors));
        }

        return module;
    }

    private static Dictionary<SimonColor, SimonColor> GenerateRandomColorMap(IRobustRandom random)
    {
        var colors = Enum.GetValues<SimonColor>().ToList();
        var shuffled = new List<SimonColor>(colors);
        random.Shuffle(shuffled);

        var map = new Dictionary<SimonColor, SimonColor>();
        for (int i = 0; i < colors.Count; i++)
        {
            map[colors[i]] = shuffled[i];
        }
        return map;
    }

    public SimonColor GetMappedColor(SimonColor flashColor, bool hasVowel, int strikes)
    {
        var strikeKey = Math.Clamp(strikes, 0, 2);
        var map = hasVowel ? VowelMappings[strikeKey] : NoVowelMappings[strikeKey];
        return map[flashColor];
    }

    public override BombDefusalModuleState GetVisibleState()
    {
        var visibleSequence = FullSequence.Take(CurrentStage + 1).ToList();

        return new SimonSaysModuleState
        {
            IsSolved = IsSolved,
            FlashSequence = visibleSequence,
            TotalStages = TotalStages,
            CurrentStage = CurrentStage,
            InputProgress = InputProgress,
            IsFlashing = true,
        };
    }

    public override bool ValidateAction(BombModuleAction action)
    {
        return false;
    }

    public bool ValidateActionWithStrikes(BombModuleAction action, int currentStrikes)
    {
        if (IsSolved)
            return true;

        if (action is not PressSimonColorAction pressColor)
            return false;

        var flashColor = FullSequence[InputProgress];
        var expectedColor = GetMappedColor(flashColor, SerialHasVowel, currentStrikes);

        if (pressColor.Color == expectedColor)
        {
            InputProgress++;

            if (InputProgress >= CurrentStage + 1)
            {
                CurrentStage++;
                InputProgress = 0;

                if (CurrentStage >= TotalStages)
                    IsSolved = true;
            }

            return true;
        }

        InputProgress = 0;
        return false;
    }
}
