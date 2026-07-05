using System.Linq;
using Content.Shared._FarHorizons.Salvage;
using Content.Shared.Procedural;
using Content.Shared.Salvage.Expeditions;
using Content.Shared.Salvage.Expeditions.Modifiers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.Salvage;

public abstract partial class SharedSalvageSystem
{
    const string FallbackObjective = "MissionObjectiveFree";
    const string FallbackWeather = "SalvageWeatherNone";

    private SalvageMissionObjectivePrototype _fallbackObjectiveProto => 
        _proto.Index<SalvageMissionObjectivePrototype>(FallbackObjective);
    
    private SalvageWeatherMod _fallbackWeatherProto => 
        _proto.Index<SalvageWeatherMod>(FallbackWeather);

    public SalvageMissionObjectivePrototype GetMissionObjective(System.Random rand, ProtoId<SalvageDifficultyPrototype> difficulty, ProtoId<SalvageBiomeModPrototype> biome, ProtoId<SalvageFactionPrototype> faction, ProtoId<SalvageDungeonModPrototype> dungeon)
    {
        var objectives = _proto.EnumeratePrototypes<SalvageMissionObjectivePrototype>()
                            .Where(p => 
                                (p.AllowedDifficulties == null || p.AllowedDifficulties.Contains(difficulty)) &&
                                (p.AllowedBiomes == null || p.AllowedBiomes.Contains(biome)) && 
                                (p.AllowedFactions == null || p.AllowedFactions.Contains(faction)) &&
                                (p.AllowedDungeons == null || p.AllowedDungeons.Contains(dungeon)))
                            .ToList();
        
        objectives.Sort((x, y) => string.Compare(x.ID, y.ID, StringComparison.Ordinal)); // yes, it is bullshit

        if (objectives.Count == 0)
            return _fallbackObjectiveProto;

        if (objectives.Count == 1)
            return objectives[0];

        rand.Shuffle(objectives);
        return objectives[0];
    }

    public SalvageWeatherMod GetWeatherMod(System.Random rand, ProtoId<SalvageDifficultyPrototype> difficulty, ProtoId<SalvageBiomeModPrototype> biome, ref float rating)
    {
        var searchRating = rating;
        var weathers = _proto.EnumeratePrototypes<SalvageWeatherMod>()
                             .Where(p => 
                                (p.Difficulties == null || p.Difficulties.Contains(difficulty)) &&
                                (p.Biomes == null || p.Biomes.Contains(biome)) &&
                                (p.Cost <= searchRating))
                             .ToList();
        
        weathers.Sort((x, y) => string.Compare(x.ID, y.ID, StringComparison.Ordinal));

        if (weathers.Count == 0)
            return _fallbackWeatherProto;
        
        if (weathers.Count > 1)
            rand.Shuffle(weathers);

        var result = weathers[0];
        rating -= result.Cost;
        
        return result;
    }
}