using System.Diagnostics.CodeAnalysis;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Random;

namespace Content.Shared.Genetics.Systems;

public abstract partial class SharedDnaModifierSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public void TrySaveInDisk(EntityUid disk, EnzymeInfo enzyme)
    {
        if (!TryComp(disk, out DnaModifierDiskComponent? comp))
            return;

        if (comp.Data != null)
            return;

        comp.Data = enzyme;

        Dirty(disk, comp);
        return;
    }

    public bool TryGetDataFromDisk(EntityUid disk, [NotNullWhen(true)] out EnzymeInfo? data)
    {
        data = null;
        if (!TryComp(disk, out DnaModifierDiskComponent? comp))
            return false;

        if (comp.Data == null)
            return false;

        data = comp.Data;
        return true;
    }

    public bool TryClearDiskData(EntityUid disk)
    {
        if (!TryComp(disk, out DnaModifierDiskComponent? comp))
            return false;

        if (comp.Data == null)
            return false;

        comp.Data = null;
        return true;
    }

    public Color GetFirstMarkingColor(IReadOnlyList<Marking> markings)
    {
        if (markings.Count > 0 && markings[0].MarkingColors.Count > 0)
        {
            return markings[0].MarkingColors[0];
        }
        return Color.White;
    }

    public string[] ConvertColorToHexArray(Color color)
    {
        int r = (int)(color.R * 255);
        int g = (int)(color.G * 255);
        int b = (int)(color.B * 255);

        string rHex = r.ToString("X2");
        string gHex = g.ToString("X2");
        string bHex = b.ToString("X2");

        return new[]
        {
            rHex[0].ToString(),
            rHex[1].ToString(),
            "0",
            gHex[0].ToString(),
            gHex[1].ToString(),
            "0",
            bHex[0].ToString(),
            bHex[1].ToString(),
            "0"
        };
    }

    public string[] ConvertSkinToneToHexArray(Color skinColor)
    {
        var hsv = Color.ToHsv(skinColor);
        var hue = hsv.X * 360f;

        const float minHue = 25f;
        const float maxHue = 45f;

        hue = Math.Clamp(hue, minHue, maxHue);

        var normalizedTone = (hue - minHue) / (maxHue - minHue) * 100f;
        var toneValue = (int)Math.Round(normalizedTone);

        var part1 = (toneValue / 16) % 16;
        var part2 = (toneValue / 1) % 16;
        var part3 = 0;

        return new[]
        {
            part1.ToString("X1"),
            part2.ToString("X1"),
            part3.ToString("X1")
        };
    }

    public string[] GenerateRandomGenderHexValue(int minHex, int maxHex)
    {
        var value = _random.Next(minHex, maxHex + 1);
        var hexString = value.ToString("X3");
        return new[]
        {
            hexString[0].ToString(),
            hexString[1].ToString(),
            hexString[2].ToString()
        };
    }

    public string[] GenerateRandomHexValues()
    {
        return new[]
        {
            _random.Next(0, 16).ToString("X1"),
            _random.Next(0, 16).ToString("X1"),
            _random.Next(0, 16).ToString("X1")
        };
    }

    public string[] GenerateRandomToneValues()
    {
        var toneValue = _random.Next(0, 101);

        var part1 = (toneValue / 16) % 16;
        var part2 = (toneValue / 1) % 16;
        var part3 = 0;

        return new[]
        {
            part1.ToString("X1"),
            part2.ToString("X1"),
            part3.ToString("X1")
        };
    }
}

