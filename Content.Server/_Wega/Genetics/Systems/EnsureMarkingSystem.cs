using System.Linq;
using Content.Server.Humanoid;
using Content.Shared.Genetics;
using Content.Shared.Genetics.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Prototypes;

namespace Content.Server.Genetics.System;

public sealed class EnsureMarkingSystem : EntitySystem
{
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;

    public static readonly ProtoId<MarkingPrototype> DefaultHorns = "LizardHornsDemonic";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EnsureHornsGenComponent, ComponentInit>(OnHornsInit);
        SubscribeLocalEvent<EnsureHornsGenComponent, ComponentShutdown>(OnHornsShutdown);
    }

    private void OnHornsInit(Entity<EnsureHornsGenComponent> ent, ref ComponentInit args)
    {
        if (TryComp<HumanoidAppearanceComponent>(ent, out _))
            _humanoid.AddMarking(ent, DefaultHorns, Color.Black);
    }

    private void OnHornsShutdown(Entity<EnsureHornsGenComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<HumanoidAppearanceComponent>(ent, out _))
            _humanoid.RemoveMarking(ent, DefaultHorns);
    }

    public void UpdateMarkingCategory(
        Entity<HumanoidAppearanceComponent> humanoid,
        MarkingSet markingSet,
        MarkingCategories category,
        string[] colorR, string[] colorG, string[] colorB,
        string[] style, string species,
        List<MarkingPrototypeInfo> markingPrototypes,
        string[]? secondaryColorR = null,
        string[]? secondaryColorG = null,
        string[]? secondaryColorB = null)
    {
        markingSet.RemoveCategory(category);
        if (style.All(c => c == "0"))
            return;

        if (category == MarkingCategories.HeadTop && HasComp<EnsureHornsGenComponent>(humanoid))
        {
            _humanoid.AddMarking(humanoid, DefaultHorns, Color.Black);
            return;
        }

        var bestMatch = FindBestMatchingMarking(style, species, markingPrototypes);
        if (bestMatch == null)
            return;

        string redHex = colorR[0] + colorR[1];
        string greenHex = colorG[0] + colorG[1];
        string blueHex = colorB[0] + colorB[1];

        int red = Convert.ToInt32(redHex, 16);
        int green = Convert.ToInt32(greenHex, 16);
        int blue = Convert.ToInt32(blueHex, 16);

        var mainColor = new Color(red / 255f, green / 255f, blue / 255f);

        var colors = new List<Color> { mainColor };

        if (category == MarkingCategories.Hair &&
            secondaryColorR != null &&
            secondaryColorG != null &&
            secondaryColorB != null)
        {
            string secondaryRedHex = secondaryColorR[0] + secondaryColorR[1];
            string secondaryGreenHex = secondaryColorG[0] + secondaryColorG[1];
            string secondaryBlueHex = secondaryColorB[0] + secondaryColorB[1];

            int secondaryRed = Convert.ToInt32(secondaryRedHex, 16);
            int secondaryGreen = Convert.ToInt32(secondaryGreenHex, 16);
            int secondaryBlue = Convert.ToInt32(secondaryBlueHex, 16);

            var secondaryColor = new Color(secondaryRed / 255f, secondaryGreen / 255f, secondaryBlue / 255f);
            colors.Add(secondaryColor);
        }

        _humanoid.AddMarkingWithColors(humanoid, bestMatch.MarkingPrototypeId, colors);
    }

    private MarkingPrototypeInfo? FindBestMatchingMarking(string[] style, string species, List<MarkingPrototypeInfo> markingPrototypes)
    {
        MarkingPrototypeInfo? bestMatch = null;
        int bestScore = int.MaxValue;

        foreach (var marking in markingPrototypes)
        {
            if (!string.IsNullOrEmpty(marking.Species) && !marking.Species.Contains(species))
                continue;

            int score = CalculateStyleMatchScore(marking.HexValue, style);
            if (score < bestScore)
            {
                bestScore = score;
                bestMatch = marking;
            }
        }

        return bestMatch;
    }

    private int CalculateStyleMatchScore(string[] markingStyle, string[] targetStyle)
    {
        int score = 0;
        for (int i = 0; i < markingStyle.Length; i++)
        {
            if (i >= targetStyle.Length)
                break;

            int markingValue = Convert.ToInt32(markingStyle[i], 16);
            int targetValue = Convert.ToInt32(targetStyle[i], 16);
            score += Math.Abs(markingValue - targetValue);
        }

        return score;
    }
}
