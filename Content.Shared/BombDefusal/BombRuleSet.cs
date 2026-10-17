using Content.Shared.BombDefusal.Modules;
using Robust.Shared.Serialization;

namespace Content.Shared.BombDefusal;

[NetSerializable, Serializable]
public sealed class BombRuleSet
{
    public string SerialNumber = string.Empty;
    public int ModuleCount;
    public Dictionary<BombModuleType, BombModuleRules> ModuleRules = new();
}

[NetSerializable, Serializable]
public sealed class BombModuleRules
{
    public string ModuleName = string.Empty;
    public List<string> RuleLines = new();
    
    // Additional data the scanner client UI can use to render custom interfaces
    // (e.g., drawing a maze map, or showing tables/symbols)
    public Dictionary<string, string> Metadata = new();
}
