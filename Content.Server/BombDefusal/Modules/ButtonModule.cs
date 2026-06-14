using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

/// <summary>
/// "The Button" module.
/// A single colored button with a label. The player must either tap it or hold
/// and release when the timer shows a specific digit, depending on rules.
/// </summary>
public sealed class ButtonModule : BombModule
{
    public ButtonColor ButtonColor;
    public ButtonLabel ButtonLabel;

    /// <summary>
    /// The color of the strip that appears when holding.
    /// </summary>
    public ButtonColor StripColor;

    /// <summary>
    /// Whether the correct action is to hold (true) or tap (false).
    /// </summary>
    public bool ShouldHold;

    /// <summary>
    /// If ShouldHold is true, the timer digit the player must release on.
    /// Determined by the strip color.
    /// </summary>
    public int ReleaseDigit;

    /// <summary>
    /// Whether the button is currently being held.
    /// </summary>
    public bool IsHeld;

    public ButtonModule()
    {
        Type = BombModuleType.Button;
    }

    public static ButtonModule Generate(IRobustRandom random, string serialNumber)
    {
        var module = new ButtonModule();
        module.ButtonColor = random.Pick(Enum.GetValues<ButtonColor>());
        module.ButtonLabel = random.Pick(Enum.GetValues<ButtonLabel>());
        module.StripColor = random.Pick(Enum.GetValues<ButtonColor>());

        // Determine if the player should hold or tap based on KTANE-style rules
        module.ShouldHold = DetermineShouldHold(module.ButtonColor, module.ButtonLabel);

        // If holding, determine release digit based on strip color
        module.ReleaseDigit = GetReleaseDigit(module.StripColor);

        return module;
    }

    /// <summary>
    /// KTANE-style rules for tap vs hold.
    /// </summary>
    private static bool DetermineShouldHold(ButtonColor color, ButtonLabel label)
    {
        // If the button is blue and says "Abort", hold.
        if (color == ButtonColor.Blue && label == ButtonLabel.Abort)
            return true;

        // If the button says "Detonate", tap (press and release).
        if (label == ButtonLabel.Detonate)
            return false;

        // If the button is white, hold.
        if (color == ButtonColor.White)
            return true;

        // If the button is red and says "Hold", tap.
        if (color == ButtonColor.Red && label == ButtonLabel.Hold)
            return false;

        // Otherwise, hold.
        return true;
    }

    /// <summary>
    /// Determine which timer digit the player must release on based on strip color.
    /// </summary>
    private static int GetReleaseDigit(ButtonColor stripColor)
    {
        return stripColor switch
        {
            ButtonColor.Blue => 4,    // Release when timer has a 4
            ButtonColor.White => 1,   // Release when timer has a 1
            ButtonColor.Yellow => 5,  // Release when timer has a 5
            ButtonColor.Red => 1,     // Release when timer has a 1
            _ => 1,
        };
    }

    public override BombDefusalModuleState GetVisibleState()
    {
        return new ButtonModuleState
        {
            IsSolved = IsSolved,
            Color = ButtonColor,
            Label = ButtonLabel,
            StripColor = IsHeld ? StripColor : null,
            IsHeld = IsHeld,
        };
    }

    public override bool ValidateAction(BombModuleAction action)
    {
        if (IsSolved)
            return true;

        switch (action)
        {
            case PressButtonAction:
                IsHeld = true;
                if (!ShouldHold)
                {
                    // The correct action is to tap — we mark as solved on release.
                    // But we need to wait for the release action.
                }
                return true; // Pressing is never wrong by itself

            case ReleaseButtonAction release:
                if (!IsHeld)
                    return true; // Wasn't held, ignore

                IsHeld = false;

                if (ShouldHold)
                {
                    // Player should have held and released on correct digit
                    if (release.TimerDigit == ReleaseDigit)
                    {
                        IsSolved = true;
                        return true;
                    }
                    return false; // Wrong release digit — strike
                }
                else
                {
                    // Player should have tapped (quick press and release)
                    // If they're releasing, that's a tap — correct!
                    IsSolved = true;
                    return true;
                }

            default:
                return false;
        }
    }
}
