using Robust.Shared.Serialization;

namespace Content.Shared.Genetics;

[Serializable, NetSerializable]
public sealed class EnzymeInfo
{
    public string SampleName { get; set; } = string.Empty;
    public UniqueIdentifiersPrototype? Identifier { get; set; }
    public List<EnzymesPrototypeInfo>? Info { get; set; }
}

[Serializable, NetSerializable]
public sealed class EnzymesPrototypeInfo
{
    public string EnzymesPrototypeId { get; set; } = string.Empty;
    public string[] HexCode { get; set; } = new[] { "0", "0", "0" };
    public int Order { get; set; } = default!;
}
