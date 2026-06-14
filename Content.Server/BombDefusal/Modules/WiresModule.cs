using System.Linq;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Random;

namespace Content.Server.BombDefusal.Modules;

/// <summary>
/// "Simple Wires" module.
/// 3-6 colored wires; the defuser must cut exactly the correct one.
/// Rules mirror KTANE's wire module logic, adapted for SS14.
/// </summary>
public sealed class WiresModule : BombModule
{
    public List<WireColor> WireColors = new();
    public HashSet<int> CutWires = new();

    /// <summary>
    /// The index of the correct wire to cut.
    /// </summary>
    public int CorrectWireIndex;

    public WiresModule()
    {
        Type = BombModuleType.Wires;
    }

    /// <summary>
    /// Generate a random Wires module.
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

        module.CorrectWireIndex = DetermineCorrectWire(module.WireColors, serialNumber);
        return module;
    }

    /// <summary>
    /// Determine which wire to cut based on KTANE-style rules.
    /// </summary>
    private static int DetermineCorrectWire(List<WireColor> wires, string serialNumber)
    {
        var lastDigitOdd = serialNumber.Length > 0 &&
                           char.IsDigit(serialNumber[^1]) &&
                           (serialNumber[^1] - '0') % 2 != 0;

        var count = wires.Count;

        switch (count)
        {
            case 3:
                // If there are no red wires, cut the second wire.
                if (!wires.Contains(WireColor.Red))
                    return 1;
                // If the last wire is white, cut the last wire.
                if (wires[^1] == WireColor.White)
                    return count - 1;
                // If there is more than one blue wire, cut the last blue wire.
                if (wires.Count(w => w == WireColor.Blue) > 1)
                    return wires.LastIndexOf(WireColor.Blue);
                // Otherwise, cut the last wire.
                return count - 1;

            case 4:
                // If there is more than one red wire and the last digit of serial is odd, cut the last red wire.
                if (wires.Count(w => w == WireColor.Red) > 1 && lastDigitOdd)
                    return wires.LastIndexOf(WireColor.Red);
                // If the last wire is yellow and there are no red wires, cut the first wire.
                if (wires[^1] == WireColor.Yellow && !wires.Contains(WireColor.Red))
                    return 0;
                // If there is exactly one blue wire, cut the first wire.
                if (wires.Count(w => w == WireColor.Blue) == 1)
                    return 0;
                // If there is more than one yellow wire, cut the last wire.
                if (wires.Count(w => w == WireColor.Yellow) > 1)
                    return count - 1;
                // Otherwise, cut the second wire.
                return 1;

            case 5:
                // If the last wire is black and the last digit is odd, cut the fourth wire.
                if (wires[^1] == WireColor.Black && lastDigitOdd)
                    return 3;
                // If there is exactly one red wire and more than one yellow wire, cut the first wire.
                if (wires.Count(w => w == WireColor.Red) == 1 && wires.Count(w => w == WireColor.Yellow) > 1)
                    return 0;
                // If there are no black wires, cut the second wire.
                if (!wires.Contains(WireColor.Black))
                    return 1;
                // Otherwise, cut the first wire.
                return 0;

            case 6:
                // If there are no yellow wires and the last digit is odd, cut the third wire.
                if (!wires.Contains(WireColor.Yellow) && lastDigitOdd)
                    return 2;
                // If there is exactly one yellow wire and more than one white wire, cut the fourth wire.
                if (wires.Count(w => w == WireColor.Yellow) == 1 && wires.Count(w => w == WireColor.White) > 1)
                    return 3;
                // If there are no red wires, cut the last wire.
                if (!wires.Contains(WireColor.Red))
                    return count - 1;
                // Otherwise, cut the fourth wire.
                return 3;

            default:
                return 0;
        }
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

        // Can't cut an already-cut wire
        if (CutWires.Contains(cutWire.WireIndex))
            return true; // Not a strike, just a no-op

        CutWires.Add(cutWire.WireIndex);

        if (cutWire.WireIndex == CorrectWireIndex)
        {
            IsSolved = true;
            return true;
        }

        return false; // Wrong wire — strike!
    }
}
