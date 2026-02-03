using Content.Shared.Genetics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Genetics;

[Prototype, Access(typeof(SharedDnaModifierSystem), typeof(EnzymeInfo))]
[Serializable, NetSerializable]
public sealed class UniqueIdentifiersPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = string.Empty;

    // Блок 1: RGB значения цвета волос (R)
    [DataField("hairColorR")]
    public string[] HairColorR { get; set; } = new[] { "0", "0", "0" };

    // Блок 2: RGB значения цвета волос (G)
    [DataField("hairColorG")]
    public string[] HairColorG { get; set; } = new[] { "0", "0", "0" };

    // Блок 3: RGB значения цвета волос (B)
    [DataField("hairColorB")]
    public string[] HairColorB { get; set; } = new[] { "0", "0", "0" };

    // Блок 4: RGB значения вторичного цвета волос (R)
    [DataField("secondaryHairColorR")]
    public string[] SecondaryHairColorR { get; set; } = new[] { "0", "0", "0" };

    // Блок 5: RGB значения вторичного цвета волос (G)
    [DataField("secondaryHairColorG")]
    public string[] SecondaryHairColorG { get; set; } = new[] { "0", "0", "0" };

    // Блок 6: RGB значения вторичного цвета волос (B)
    [DataField("secondaryHairColorB")]
    public string[] SecondaryHairColorB { get; set; } = new[] { "0", "0", "0" };

    // Блок 7: RGB значения цвета бороды (R)
    [DataField("beardColorR")]
    public string[] BeardColorR { get; set; } = new[] { "0", "0", "0" };

    // Блок 8: RGB значения цвета бороды (G)
    [DataField("beardColorG")]
    public string[] BeardColorG { get; set; } = new[] { "0", "0", "0" };

    // Блок 9: RGB значения цвета бороды (B)
    [DataField("beardColorB")]
    public string[] BeardColorB { get; set; } = new[] { "0", "0", "0" };

    /* Этого блока пока быть не должно
    // Блок 10: RGB значения цвета вторичной бороды (R)
    [DataField("secondaryBeardColorR")]
    public string[] SecondaryBeardColorR { get; set; } = new[] { "0", "0", "0" };

    // Блок 11: RGB значения цвета вторичной бороды (G)
    [DataField("secondaryBeardColorG")]
    public string[] SecondaryBeardColorG { get; set; } = new[] { "0", "0", "0" };

    // Блок 12: RGB значения цвета вторичной бороды (B)
    [DataField("secondaryBeardColorB")]
    public string[] SecondaryBeardColorB { get; set; } = new[] { "0", "0", "0" };
    */

    // Блок 13: Тон кожи (1-220)
    [DataField("skinTone")]
    public string[] SkinTone { get; set; } = new[] { "0", "0", "0" };

    // Блок 14: RGB значения цвета меха (R)
    [DataField("furColorR")]
    public string[] FurColorR { get; set; } = new[] { "0", "0", "0" };

    // Блок 15: RGB значения цвета меха (G)
    [DataField("furColorG")]
    public string[] FurColorG { get; set; } = new[] { "0", "0", "0" };

    // Блок 16: RGB значения цвета меха (B)
    [DataField("furColorB")]
    public string[] FurColorB { get; set; } = new[] { "0", "0", "0" };

    // Блок 17: RGB значения головного аксессуара (R)
    [DataField("headAccessoryColorR")]
    public string[] HeadAccessoryColorR { get; set; } = new[] { "0", "0", "0" };

    // Блок 18: RGB значения головного аксессуара (G)
    [DataField("headAccessoryColorG")]
    public string[] HeadAccessoryColorG { get; set; } = new[] { "0", "0", "0" };

    // Блок 19: RGB значения головного аксессуара (B)
    [DataField("headAccessoryColorB")]
    public string[] HeadAccessoryColorB { get; set; } = new[] { "0", "0", "0" };

    // Блок 20: RGB значения разметки головы (R)
    [DataField("headMarkingColorR")]
    public string[] HeadMarkingColorR { get; set; } = new[] { "0", "0", "0" };

    // Блок 21: RGB значения разметки головы (G)
    [DataField("headMarkingColorG")]
    public string[] HeadMarkingColorG { get; set; } = new[] { "0", "0", "0" };

    // Блок 22: RGB значения разметки головы (B)
    [DataField("headMarkingColorB")]
    public string[] HeadMarkingColorB { get; set; } = new[] { "0", "0", "0" };

    // Блок 23: RGB значения маркировки тела (R)
    [DataField("bodyMarkingColorR")]
    public string[] BodyMarkingColorR { get; set; } = new[] { "0", "0", "0" };

    // Блок 24: RGB значения маркировки тела (G)
    [DataField("bodyMarkingColorG")]
    public string[] BodyMarkingColorG { get; set; } = new[] { "0", "0", "0" };

    // Блок 25: RGB значения маркировки тела (B)
    [DataField("bodyMarkingColorB")]
    public string[] BodyMarkingColorB { get; set; } = new[] { "0", "0", "0" };

    // Блок 26: RGB значения маркировки хвоста (R)
    [DataField("tailMarkingColorR")]
    public string[] TailMarkingColorR { get; set; } = new[] { "0", "0", "0" };

    // Блок 27: RGB значения маркировки хвоста (G)
    [DataField("tailMarkingColorG")]
    public string[] TailMarkingColorG { get; set; } = new[] { "0", "0", "0" };

    // Блок 28: RGB значения маркировки хвоста (B)
    [DataField("tailMarkingColorB")]
    public string[] TailMarkingColorB { get; set; } = new[] { "0", "0", "0" };

    // Блок 29: RGB значения цвета глаз (R)
    [DataField("eyeColorR")]
    public string[] EyeColorR { get; set; } = new[] { "0", "0", "0" };

    // Блок 30: RGB значения цвета глаз (G)
    [DataField("eyeColorG")]
    public string[] EyeColorG { get; set; } = new[] { "0", "0", "0" };

    // Блок 31: RGB значения цвета глаз (B)
    [DataField("eyeColorB")]
    public string[] EyeColorB { get; set; } = new[] { "0", "0", "0" };

    // Блок 32: Пол
    [DataField("gender")]
    public string[] Gender { get; set; } = new[] { "5", "7", "3" }; // По умолчанию женщина

    // Блок 33: Стиль бороды
    [DataField("beardStyle")]
    public string[] BeardStyle { get; set; } = default!;

    // Блок 34: Стиль волос
    [DataField("hairStyle")]
    public string[] HairStyle { get; set; } = default!;

    // Блок 35: Стиль аксессуаров для головы
    [DataField("headAccessoryStyle")]
    public string[] HeadAccessoryStyle { get; set; } = default!;

    // Блок 36: Стиль маркировки головы
    [DataField("headMarkingStyle")]
    public string[] HeadMarkingStyle { get; set; } = default!;

    // Блок 37: Стиль маркировки тела
    [DataField("bodyMarkingStyle")]
    public string[] BodyMarkingStyle { get; set; } = default!;

    // Блок 38: Стиль маркировки хвоста
    [DataField("tailMarkingStyle")]
    public string[] TailMarkingStyle { get; set; } = default!;

    public object Clone()
    {
        var clone = new UniqueIdentifiersPrototype
        {
            ID = this.ID,
            HairColorR = (string[])this.HairColorR.Clone(),
            HairColorG = (string[])this.HairColorG.Clone(),
            HairColorB = (string[])this.HairColorB.Clone(),
            SecondaryHairColorR = (string[])this.SecondaryHairColorR.Clone(),
            SecondaryHairColorG = (string[])this.SecondaryHairColorG.Clone(),
            SecondaryHairColorB = (string[])this.SecondaryHairColorB.Clone(),
            BeardColorR = (string[])this.BeardColorR.Clone(),
            BeardColorG = (string[])this.BeardColorG.Clone(),
            BeardColorB = (string[])this.BeardColorB.Clone(),
            SkinTone = (string[])this.SkinTone.Clone(),
            FurColorR = (string[])this.FurColorR.Clone(),
            FurColorG = (string[])this.FurColorG.Clone(),
            FurColorB = (string[])this.FurColorB.Clone(),
            HeadAccessoryColorR = (string[])this.HeadAccessoryColorR.Clone(),
            HeadAccessoryColorG = (string[])this.HeadAccessoryColorG.Clone(),
            HeadAccessoryColorB = (string[])this.HeadAccessoryColorB.Clone(),
            HeadMarkingColorR = (string[])this.HeadMarkingColorR.Clone(),
            HeadMarkingColorG = (string[])this.HeadMarkingColorG.Clone(),
            HeadMarkingColorB = (string[])this.HeadMarkingColorB.Clone(),
            BodyMarkingColorR = (string[])this.BodyMarkingColorR.Clone(),
            BodyMarkingColorG = (string[])this.BodyMarkingColorG.Clone(),
            BodyMarkingColorB = (string[])this.BodyMarkingColorB.Clone(),
            TailMarkingColorR = (string[])this.TailMarkingColorR.Clone(),
            TailMarkingColorG = (string[])this.TailMarkingColorG.Clone(),
            TailMarkingColorB = (string[])this.TailMarkingColorB.Clone(),
            EyeColorR = (string[])this.EyeColorR.Clone(),
            EyeColorG = (string[])this.EyeColorG.Clone(),
            EyeColorB = (string[])this.EyeColorB.Clone(),
            Gender = (string[])this.Gender.Clone(),
            BeardStyle = (string[])this.BeardStyle.Clone(),
            HairStyle = (string[])this.HairStyle.Clone(),
            HeadAccessoryStyle = (string[])this.HeadAccessoryStyle.Clone(),
            HeadMarkingStyle = (string[])this.HeadMarkingStyle.Clone(),
            BodyMarkingStyle = (string[])this.BodyMarkingStyle.Clone(),
            TailMarkingStyle = (string[])this.TailMarkingStyle.Clone()
        };

        return clone;
    }
}
