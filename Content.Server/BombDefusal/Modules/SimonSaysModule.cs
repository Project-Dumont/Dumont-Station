using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

/// <summary>
/// "Simon Says" module.
/// Colored lights flash in a sequence; the player must press the
/// remapped colors. The mapping depends on the serial number and strike count.
/// </summary>
public sealed class SimonSaysModule : BombModule
{
    /// <summary>
    /// The full sequence of flash colors (all stages combined).
    /// Stage N shows the first N+1 colors of this sequence.
    /// </summary>
    public List<SimonColor> FullSequence = new();

    /// <summary>
    /// Total number of stages to complete.
    /// </summary>
    public int TotalStages;

    /// <summary>
    /// Current stage (0-indexed).
    /// </summary>
    public int CurrentStage;

    /// <summary>
    /// How many colors the player has correctly input in the current stage.
    /// </summary>
    public int InputProgress;

    /// <summary>
    /// Whether the serial number contains a vowel (affects color mapping).
    /// </summary>
    public bool SerialHasVowel;

    public SimonSaysModule()
    {
        Type = BombModuleType.SimonSays;
    }

    public static SimonSaysModule Generate(IRobustRandom random, string serialNumber)
    {
        var module = new SimonSaysModule();
        module.TotalStages = random.Next(3, 6); // 3 to 5 stages
        module.SerialHasVowel = serialNumber.Any(c => "AEIOUaeiou".Contains(c));

        // Generate the full sequence
        var colors = Enum.GetValues<SimonColor>();
        for (var i = 0; i < module.TotalStages; i++)
        {
            module.FullSequence.Add(random.Pick(colors));
        }

        return module;
    }

    /// <summary>
    /// Get the correct button to press for a given flash color,
    /// based on serial vowel presence and current strike count.
    /// </summary>
    public static SimonColor GetMappedColor(SimonColor flashColor, bool hasVowel, int strikes)
    {
        // KTANE-style color mapping tables
        if (hasVowel)
        {
            return strikes switch
            {
                0 => flashColor switch
                {
                    SimonColor.Red => SimonColor.Blue,
                    SimonColor.Blue => SimonColor.Red,
                    SimonColor.Green => SimonColor.Yellow,
                    SimonColor.Yellow => SimonColor.Green,
                    _ => flashColor,
                },
                1 => flashColor switch
                {
                    SimonColor.Red => SimonColor.Yellow,
                    SimonColor.Blue => SimonColor.Green,
                    SimonColor.Green => SimonColor.Blue,
                    SimonColor.Yellow => SimonColor.Red,
                    _ => flashColor,
                },
                _ => flashColor switch // 2+ strikes
                {
                    SimonColor.Red => SimonColor.Green,
                    SimonColor.Blue => SimonColor.Red,
                    SimonColor.Green => SimonColor.Yellow,
                    SimonColor.Yellow => SimonColor.Blue,
                    _ => flashColor,
                },
            };
        }
        else
        {
            return strikes switch
            {
                0 => flashColor switch
                {
                    SimonColor.Red => SimonColor.Blue,
                    SimonColor.Blue => SimonColor.Yellow,
                    SimonColor.Green => SimonColor.Green,
                    SimonColor.Yellow => SimonColor.Red,
                    _ => flashColor,
                },
                1 => flashColor switch
                {
                    SimonColor.Red => SimonColor.Red,
                    SimonColor.Blue => SimonColor.Blue,
                    SimonColor.Green => SimonColor.Yellow,
                    SimonColor.Yellow => SimonColor.Green,
                    _ => flashColor,
                },
                _ => flashColor switch // 2+ strikes
                {
                    SimonColor.Red => SimonColor.Yellow,
                    SimonColor.Blue => SimonColor.Green,
                    SimonColor.Green => SimonColor.Blue,
                    SimonColor.Yellow => SimonColor.Red,
                    _ => flashColor,
                },
            };
        }
    }

    public override BombDefusalModuleState GetVisibleState()
    {
        // Only show the sequence up to current stage + 1
        var visibleSequence = FullSequence.Take(CurrentStage + 1).ToList();

        return new SimonSaysModuleState
        {
            IsSolved = IsSolved,
            FlashSequence = visibleSequence,
            TotalStages = TotalStages,
            CurrentStage = CurrentStage,
            InputProgress = InputProgress,
            IsFlashing = true, // Client handles the animation timing
        };
    }

    /// <summary>
    /// Validate a color press. The caller must pass in the current strike count
    /// since the mapping depends on it. This is handled by using a two-step process:
    /// the BombDefusalSystem calls ValidateActionWithStrikes instead.
    /// </summary>
    public override bool ValidateAction(BombModuleAction action)
    {
        // This should not be called directly — use ValidateActionWithStrikes
        return false;
    }

    /// <summary>
    /// Validate with strike count context.
    /// </summary>
    public bool ValidateActionWithStrikes(BombModuleAction action, int currentStrikes)
    {
        if (IsSolved)
            return true;

        if (action is not PressSimonColorAction pressColor)
            return false;

        // The flash color at the current input position
        var flashColor = FullSequence[InputProgress];
        var expectedColor = GetMappedColor(flashColor, SerialHasVowel, currentStrikes);

        if (pressColor.Color == expectedColor)
        {
            InputProgress++;

            // Completed all presses for this stage?
            if (InputProgress >= CurrentStage + 1)
            {
                CurrentStage++;
                InputProgress = 0;

                if (CurrentStage >= TotalStages)
                    IsSolved = true;
            }

            return true;
        }

        // Wrong color — strike! Reset current stage progress.
        InputProgress = 0;
        return false;
    }
}
