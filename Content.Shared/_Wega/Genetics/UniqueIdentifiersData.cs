using Content.Shared.Genetics.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared.Genetics;

[Serializable, NetSerializable]
[Access(typeof(SharedDnaModifierSystem), typeof(EnzymeInfo))]
public sealed class UniqueIdentifiersData
{
    public string ID { get; set; } = string.Empty;
    public string[] HairColorR { get; set; } = Array.Empty<string>();
    public string[] HairColorG { get; set; } = Array.Empty<string>();
    public string[] HairColorB { get; set; } = Array.Empty<string>();
    public string[] SecondaryHairColorR { get; set; } = Array.Empty<string>();
    public string[] SecondaryHairColorG { get; set; } = Array.Empty<string>();
    public string[] SecondaryHairColorB { get; set; } = Array.Empty<string>();
    public string[] BeardColorR { get; set; } = Array.Empty<string>();
    public string[] BeardColorG { get; set; } = Array.Empty<string>();
    public string[] BeardColorB { get; set; } = Array.Empty<string>();
    public string[] SkinTone { get; set; } = new[] { "0", "0", "0" };
    public string[] FurColorR { get; set; } = new[] { "0", "0", "0" };
    public string[] FurColorG { get; set; } = new[] { "0", "0", "0" };
    public string[] FurColorB { get; set; } = new[] { "0", "0", "0" };
    public string[] HeadAccessoryColorR { get; set; } = Array.Empty<string>();
    public string[] HeadAccessoryColorG { get; set; } = Array.Empty<string>();
    public string[] HeadAccessoryColorB { get; set; } = Array.Empty<string>();
    public string[] HeadMarkingColorR { get; set; } = Array.Empty<string>();
    public string[] HeadMarkingColorG { get; set; } = Array.Empty<string>();
    public string[] HeadMarkingColorB { get; set; } = Array.Empty<string>();
    public string[] BodyMarkingColorR { get; set; } = Array.Empty<string>();
    public string[] BodyMarkingColorG { get; set; } = Array.Empty<string>();
    public string[] BodyMarkingColorB { get; set; } = Array.Empty<string>();
    public string[] TailMarkingColorR { get; set; } = Array.Empty<string>();
    public string[] TailMarkingColorG { get; set; } = Array.Empty<string>();
    public string[] TailMarkingColorB { get; set; } = Array.Empty<string>();
    public string[] EyeColorR { get; set; } = Array.Empty<string>();
    public string[] EyeColorG { get; set; } = Array.Empty<string>();
    public string[] EyeColorB { get; set; } = Array.Empty<string>();
    public string[] Gender { get; set; } = Array.Empty<string>();
    public string[] HairStyle { get; set; } = Array.Empty<string>();
    public string[] BeardStyle { get; set; } = Array.Empty<string>();
    public string[] HeadAccessoryStyle { get; set; } = Array.Empty<string>();
    public string[] HeadMarkingStyle { get; set; } = Array.Empty<string>();
    public string[] BodyMarkingStyle { get; set; } = Array.Empty<string>();
    public string[] TailMarkingStyle { get; set; } = Array.Empty<string>();

    public UniqueIdentifiersData Clone(UniqueIdentifiersData data)
    {
        var newData = new UniqueIdentifiersData()
        {
            ID = data.ID,
            HairColorR = (string[])data.HairColorR.Clone(),
            HairColorG = (string[])data.HairColorG.Clone(),
            HairColorB = (string[])data.HairColorB.Clone(),
            SecondaryHairColorR = (string[])data.SecondaryHairColorR.Clone(),
            SecondaryHairColorG = (string[])data.SecondaryHairColorG.Clone(),
            SecondaryHairColorB = (string[])data.SecondaryHairColorB.Clone(),
            BeardColorR = (string[])data.BeardColorR.Clone(),
            BeardColorG = (string[])data.BeardColorG.Clone(),
            BeardColorB = (string[])data.BeardColorB.Clone(),
            SkinTone = (string[])data.SkinTone.Clone(),
            FurColorR = (string[])data.FurColorR.Clone(),
            FurColorG = (string[])data.FurColorG.Clone(),
            FurColorB = (string[])data.FurColorB.Clone(),
            HeadAccessoryColorR = (string[])data.HeadAccessoryColorR.Clone(),
            HeadAccessoryColorG = (string[])data.HeadAccessoryColorG.Clone(),
            HeadAccessoryColorB = (string[])data.HeadAccessoryColorB.Clone(),
            HeadMarkingColorR = (string[])data.HeadMarkingColorR.Clone(),
            HeadMarkingColorG = (string[])data.HeadMarkingColorG.Clone(),
            HeadMarkingColorB = (string[])data.HeadMarkingColorB.Clone(),
            BodyMarkingColorR = (string[])data.BodyMarkingColorR.Clone(),
            BodyMarkingColorG = (string[])data.BodyMarkingColorG.Clone(),
            BodyMarkingColorB = (string[])data.BodyMarkingColorB.Clone(),
            TailMarkingColorR = (string[])data.TailMarkingColorR.Clone(),
            TailMarkingColorG = (string[])data.TailMarkingColorG.Clone(),
            TailMarkingColorB = (string[])data.TailMarkingColorB.Clone(),
            EyeColorR = (string[])data.EyeColorR.Clone(),
            EyeColorG = (string[])data.EyeColorG.Clone(),
            EyeColorB = (string[])data.EyeColorB.Clone(),
            Gender = (string[])data.Gender.Clone(),
            BeardStyle = (string[])data.BeardStyle.Clone(),
            HairStyle = (string[])data.HairStyle.Clone(),
            HeadAccessoryStyle = (string[])data.HeadAccessoryStyle.Clone(),
            HeadMarkingStyle = (string[])data.HeadMarkingStyle.Clone(),
            BodyMarkingStyle = (string[])data.BodyMarkingStyle.Clone(),
            TailMarkingStyle = (string[])data.TailMarkingStyle.Clone()
        };

        return newData;
    }
}
