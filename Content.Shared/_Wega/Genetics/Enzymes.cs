using System.Linq;
using Robust.Shared.Serialization;

namespace Content.Shared.Genetics;

[Serializable, NetSerializable]
public sealed class EnzymeInfo
{
    public string SampleName { get; set; } = string.Empty;
    public UniqueIdentifiersPrototype? Identifier { get; set; }
    public List<EnzymesPrototypeInfo>? Info { get; set; }

    public object Clone()
    {
        return new EnzymeInfo
        {
            SampleName = this.SampleName,
            Identifier = this.Identifier != null
                ? (UniqueIdentifiersPrototype)this.Identifier.Clone()
                : null,
            Info = this.Info?.Select(e => (EnzymesPrototypeInfo)e.Clone()).ToList()
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
            EnzymesPrototypeId = this.EnzymesPrototypeId,
            HexCode = (string[])this.HexCode.Clone(),
            Order = this.Order
        };
    }
}
