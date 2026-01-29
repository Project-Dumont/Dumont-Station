using System.Linq;
using Robust.Shared.Serialization;

namespace Content.Shared.Genetics;

[Serializable, NetSerializable]
public sealed class EnzymeInfo
{
    public string SampleName { get; set; } = string.Empty;
    public UniqueIdentifiersData? Identifier { get; set; }
    public List<EnzymesPrototypeInfo>? Info { get; set; }

    public object Clone()
    {
        return new EnzymeInfo
        {
            SampleName = SampleName,
            Identifier = Identifier != null ? Identifier.Clone(Identifier) : null,
            Info = Info?.Select(e => (EnzymesPrototypeInfo)e.Clone()).ToList()
        };
    }
}

[Serializable, NetSerializable]
public sealed class EnzymesPrototypeInfo
{
    public string EnzymesPrototypeId { get; set; } = string.Empty;
    public string[] HexCode { get; set; } = new[] { "0", "0", "0" };
    public int Order { get; set; } = default!;

    public object Clone()
    {
        return new EnzymesPrototypeInfo
        {
            EnzymesPrototypeId = EnzymesPrototypeId,
            HexCode = (string[])HexCode.Clone(),
            Order = Order
        };
    }
}
