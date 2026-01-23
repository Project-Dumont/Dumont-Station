using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server._Gabystation;

// A existência disso é terrível...
public static class GabyConstants
{
    // Não sei onde colocar isso. Os GameRuleSystem<T> são genéricos, não da pra por neles.
    public static readonly ProtoId<TagPrototype> GameDirectorRuleTag = "GameDirectorRule";
}
