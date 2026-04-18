// SPDX-FileCopyrightText: 2026 Punker Corps <punkercorps@gmail.com>
using Content.Shared.Genetics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Genetics;

[Prototype, Access(typeof(SharedDnaModifierSystem), typeof(EnzymeInfo))]
public sealed partial class UniqueIdentifiersPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = string.Empty;

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 1: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ²ÃÂ¾ÃÂ»ÃÂ¾Ã‘Â (R)
    [DataField("hairColorR")]
    public string[] HairColorR { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 2: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ²ÃÂ¾ÃÂ»ÃÂ¾Ã‘Â (G)
    [DataField("hairColorG")]
    public string[] HairColorG { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 3: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ²ÃÂ¾ÃÂ»ÃÂ¾Ã‘Â (B)
    [DataField("hairColorB")]
    public string[] HairColorB { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 4: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ²Ã‘â€šÃÂ¾Ã‘â‚¬ÃÂ¸Ã‘â€¡ÃÂ½ÃÂ¾ÃÂ³ÃÂ¾ Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ²ÃÂ¾ÃÂ»ÃÂ¾Ã‘Â (R)
    [DataField("secondaryHairColorR")]
    public string[] SecondaryHairColorR { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 5: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ²Ã‘â€šÃÂ¾Ã‘â‚¬ÃÂ¸Ã‘â€¡ÃÂ½ÃÂ¾ÃÂ³ÃÂ¾ Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ²ÃÂ¾ÃÂ»ÃÂ¾Ã‘Â (G)
    [DataField("secondaryHairColorG")]
    public string[] SecondaryHairColorG { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 6: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ²Ã‘â€šÃÂ¾Ã‘â‚¬ÃÂ¸Ã‘â€¡ÃÂ½ÃÂ¾ÃÂ³ÃÂ¾ Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ²ÃÂ¾ÃÂ»ÃÂ¾Ã‘Â (B)
    [DataField("secondaryHairColorB")]
    public string[] SecondaryHairColorB { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 7: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ±ÃÂ¾Ã‘â‚¬ÃÂ¾ÃÂ´Ã‘â€¹ (R)
    [DataField("beardColorR")]
    public string[] BeardColorR { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 8: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ±ÃÂ¾Ã‘â‚¬ÃÂ¾ÃÂ´Ã‘â€¹ (G)
    [DataField("beardColorG")]
    public string[] BeardColorG { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 9: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ±ÃÂ¾Ã‘â‚¬ÃÂ¾ÃÂ´Ã‘â€¹ (B)
    [DataField("beardColorB")]
    public string[] BeardColorB { get; set; } = new[] { "0", "0", "0" };

    /* ÃÂ­Ã‘â€šÃÂ¾ÃÂ³ÃÂ¾ ÃÂ±ÃÂ»ÃÂ¾ÃÂºÃÂ° ÃÂ¿ÃÂ¾ÃÂºÃÂ° ÃÂ±Ã‘â€¹Ã‘â€šÃ‘Å’ ÃÂ½ÃÂµ ÃÂ´ÃÂ¾ÃÂ»ÃÂ¶ÃÂ½ÃÂ¾
    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 10: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ²Ã‘â€šÃÂ¾Ã‘â‚¬ÃÂ¸Ã‘â€¡ÃÂ½ÃÂ¾ÃÂ¹ ÃÂ±ÃÂ¾Ã‘â‚¬ÃÂ¾ÃÂ´Ã‘â€¹ (R)
    [DataField("secondaryBeardColorR")]
    public string[] SecondaryBeardColorR { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 11: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ²Ã‘â€šÃÂ¾Ã‘â‚¬ÃÂ¸Ã‘â€¡ÃÂ½ÃÂ¾ÃÂ¹ ÃÂ±ÃÂ¾Ã‘â‚¬ÃÂ¾ÃÂ´Ã‘â€¹ (G)
    [DataField("secondaryBeardColorG")]
    public string[] SecondaryBeardColorG { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 12: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ²Ã‘â€šÃÂ¾Ã‘â‚¬ÃÂ¸Ã‘â€¡ÃÂ½ÃÂ¾ÃÂ¹ ÃÂ±ÃÂ¾Ã‘â‚¬ÃÂ¾ÃÂ´Ã‘â€¹ (B)
    [DataField("secondaryBeardColorB")]
    public string[] SecondaryBeardColorB { get; set; } = new[] { "0", "0", "0" };
    */

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 13: ÃÂ¢ÃÂ¾ÃÂ½ ÃÂºÃÂ¾ÃÂ¶ÃÂ¸ (1-220)
    [DataField("skinTone")]
    public string[] SkinTone { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 14: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ¼ÃÂµÃ‘â€¦ÃÂ° (R)
    [DataField("furColorR")]
    public string[] FurColorR { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 15: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ¼ÃÂµÃ‘â€¦ÃÂ° (G)
    [DataField("furColorG")]
    public string[] FurColorG { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 16: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ¼ÃÂµÃ‘â€¦ÃÂ° (B)
    [DataField("furColorB")]
    public string[] FurColorB { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 17: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ³ÃÂ¾ÃÂ»ÃÂ¾ÃÂ²ÃÂ½ÃÂ¾ÃÂ³ÃÂ¾ ÃÂ°ÃÂºÃ‘ÂÃÂµÃ‘ÂÃ‘ÂÃ‘Æ’ÃÂ°Ã‘â‚¬ÃÂ° (R)
    [DataField("headAccessoryColorR")]
    public string[] HeadAccessoryColorR { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 18: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ³ÃÂ¾ÃÂ»ÃÂ¾ÃÂ²ÃÂ½ÃÂ¾ÃÂ³ÃÂ¾ ÃÂ°ÃÂºÃ‘ÂÃÂµÃ‘ÂÃ‘ÂÃ‘Æ’ÃÂ°Ã‘â‚¬ÃÂ° (G)
    [DataField("headAccessoryColorG")]
    public string[] HeadAccessoryColorG { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 19: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ³ÃÂ¾ÃÂ»ÃÂ¾ÃÂ²ÃÂ½ÃÂ¾ÃÂ³ÃÂ¾ ÃÂ°ÃÂºÃ‘ÂÃÂµÃ‘ÂÃ‘ÂÃ‘Æ’ÃÂ°Ã‘â‚¬ÃÂ° (B)
    [DataField("headAccessoryColorB")]
    public string[] HeadAccessoryColorB { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 20: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â‚¬ÃÂ°ÃÂ·ÃÂ¼ÃÂµÃ‘â€šÃÂºÃÂ¸ ÃÂ³ÃÂ¾ÃÂ»ÃÂ¾ÃÂ²Ã‘â€¹ (R)
    [DataField("headMarkingColorR")]
    public string[] HeadMarkingColorR { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 21: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â‚¬ÃÂ°ÃÂ·ÃÂ¼ÃÂµÃ‘â€šÃÂºÃÂ¸ ÃÂ³ÃÂ¾ÃÂ»ÃÂ¾ÃÂ²Ã‘â€¹ (G)
    [DataField("headMarkingColorG")]
    public string[] HeadMarkingColorG { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 22: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â‚¬ÃÂ°ÃÂ·ÃÂ¼ÃÂµÃ‘â€šÃÂºÃÂ¸ ÃÂ³ÃÂ¾ÃÂ»ÃÂ¾ÃÂ²Ã‘â€¹ (B)
    [DataField("headMarkingColorB")]
    public string[] HeadMarkingColorB { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 23: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ¼ÃÂ°Ã‘â‚¬ÃÂºÃÂ¸Ã‘â‚¬ÃÂ¾ÃÂ²ÃÂºÃÂ¸ Ã‘â€šÃÂµÃÂ»ÃÂ° (R)
    [DataField("bodyMarkingColorR")]
    public string[] BodyMarkingColorR { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 24: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ¼ÃÂ°Ã‘â‚¬ÃÂºÃÂ¸Ã‘â‚¬ÃÂ¾ÃÂ²ÃÂºÃÂ¸ Ã‘â€šÃÂµÃÂ»ÃÂ° (G)
    [DataField("bodyMarkingColorG")]
    public string[] BodyMarkingColorG { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 25: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ¼ÃÂ°Ã‘â‚¬ÃÂºÃÂ¸Ã‘â‚¬ÃÂ¾ÃÂ²ÃÂºÃÂ¸ Ã‘â€šÃÂµÃÂ»ÃÂ° (B)
    [DataField("bodyMarkingColorB")]
    public string[] BodyMarkingColorB { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 26: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ¼ÃÂ°Ã‘â‚¬ÃÂºÃÂ¸Ã‘â‚¬ÃÂ¾ÃÂ²ÃÂºÃÂ¸ Ã‘â€¦ÃÂ²ÃÂ¾Ã‘ÂÃ‘â€šÃÂ° (R)
    [DataField("tailMarkingColorR")]
    public string[] TailMarkingColorR { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 27: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ¼ÃÂ°Ã‘â‚¬ÃÂºÃÂ¸Ã‘â‚¬ÃÂ¾ÃÂ²ÃÂºÃÂ¸ Ã‘â€¦ÃÂ²ÃÂ¾Ã‘ÂÃ‘â€šÃÂ° (G)
    [DataField("tailMarkingColorG")]
    public string[] TailMarkingColorG { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 28: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â ÃÂ¼ÃÂ°Ã‘â‚¬ÃÂºÃÂ¸Ã‘â‚¬ÃÂ¾ÃÂ²ÃÂºÃÂ¸ Ã‘â€¦ÃÂ²ÃÂ¾Ã‘ÂÃ‘â€šÃÂ° (B)
    [DataField("tailMarkingColorB")]
    public string[] TailMarkingColorB { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 29: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ³ÃÂ»ÃÂ°ÃÂ· (R)
    [DataField("eyeColorR")]
    public string[] EyeColorR { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 30: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ³ÃÂ»ÃÂ°ÃÂ· (G)
    [DataField("eyeColorG")]
    public string[] EyeColorG { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 31: RGB ÃÂ·ÃÂ½ÃÂ°Ã‘â€¡ÃÂµÃÂ½ÃÂ¸Ã‘Â Ã‘â€ ÃÂ²ÃÂµÃ‘â€šÃÂ° ÃÂ³ÃÂ»ÃÂ°ÃÂ· (B)
    [DataField("eyeColorB")]
    public string[] EyeColorB { get; set; } = new[] { "0", "0", "0" };

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 32: ÃÅ¸ÃÂ¾ÃÂ»
    [DataField("gender")]
    public string[] Gender { get; set; } = new[] { "5", "7", "3" }; // ÃÅ¸ÃÂ¾ Ã‘Æ’ÃÂ¼ÃÂ¾ÃÂ»Ã‘â€¡ÃÂ°ÃÂ½ÃÂ¸Ã‘Å½ ÃÂ¶ÃÂµÃÂ½Ã‘â€°ÃÂ¸ÃÂ½ÃÂ°

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 33: ÃÂ¡Ã‘â€šÃÂ¸ÃÂ»Ã‘Å’ ÃÂ±ÃÂ¾Ã‘â‚¬ÃÂ¾ÃÂ´Ã‘â€¹
    [DataField("beardStyle")]
    public string[] BeardStyle { get; set; } = default!;

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 34: ÃÂ¡Ã‘â€šÃÂ¸ÃÂ»Ã‘Å’ ÃÂ²ÃÂ¾ÃÂ»ÃÂ¾Ã‘Â
    [DataField("hairStyle")]
    public string[] HairStyle { get; set; } = default!;

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 35: ÃÂ¡Ã‘â€šÃÂ¸ÃÂ»Ã‘Å’ ÃÂ°ÃÂºÃ‘ÂÃÂµÃ‘ÂÃ‘ÂÃ‘Æ’ÃÂ°Ã‘â‚¬ÃÂ¾ÃÂ² ÃÂ´ÃÂ»Ã‘Â ÃÂ³ÃÂ¾ÃÂ»ÃÂ¾ÃÂ²Ã‘â€¹
    [DataField("headAccessoryStyle")]
    public string[] HeadAccessoryStyle { get; set; } = default!;

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 36: ÃÂ¡Ã‘â€šÃÂ¸ÃÂ»Ã‘Å’ ÃÂ¼ÃÂ°Ã‘â‚¬ÃÂºÃÂ¸Ã‘â‚¬ÃÂ¾ÃÂ²ÃÂºÃÂ¸ ÃÂ³ÃÂ¾ÃÂ»ÃÂ¾ÃÂ²Ã‘â€¹
    [DataField("headMarkingStyle")]
    public string[] HeadMarkingStyle { get; set; } = default!;

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 37: ÃÂ¡Ã‘â€šÃÂ¸ÃÂ»Ã‘Å’ ÃÂ¼ÃÂ°Ã‘â‚¬ÃÂºÃÂ¸Ã‘â‚¬ÃÂ¾ÃÂ²ÃÂºÃÂ¸ Ã‘â€šÃÂµÃÂ»ÃÂ°
    [DataField("bodyMarkingStyle")]
    public string[] BodyMarkingStyle { get; set; } = default!;

    // Ãâ€˜ÃÂ»ÃÂ¾ÃÂº 38: ÃÂ¡Ã‘â€šÃÂ¸ÃÂ»Ã‘Å’ ÃÂ¼ÃÂ°Ã‘â‚¬ÃÂºÃÂ¸Ã‘â‚¬ÃÂ¾ÃÂ²ÃÂºÃÂ¸ Ã‘â€¦ÃÂ²ÃÂ¾Ã‘ÂÃ‘â€šÃÂ°
    [DataField("tailMarkingStyle")]
    public string[] TailMarkingStyle { get; set; } = default!;

    public object Clone()
    {
        var clone = (UniqueIdentifiersPrototype) MemberwiseClone();

        clone.HairColorR = (string[]) HairColorR.Clone();
        clone.HairColorG = (string[]) HairColorG.Clone();
        clone.HairColorB = (string[]) HairColorB.Clone();
        clone.SecondaryHairColorR = (string[]) SecondaryHairColorR.Clone();
        clone.SecondaryHairColorG = (string[]) SecondaryHairColorG.Clone();
        clone.SecondaryHairColorB = (string[]) SecondaryHairColorB.Clone();
        clone.BeardColorR = (string[]) BeardColorR.Clone();
        clone.BeardColorG = (string[]) BeardColorG.Clone();
        clone.BeardColorB = (string[]) BeardColorB.Clone();
        clone.SkinTone = (string[]) SkinTone.Clone();
        clone.FurColorR = (string[]) FurColorR.Clone();
        clone.FurColorG = (string[]) FurColorG.Clone();
        clone.FurColorB = (string[]) FurColorB.Clone();
        clone.HeadAccessoryColorR = (string[]) HeadAccessoryColorR.Clone();
        clone.HeadAccessoryColorG = (string[]) HeadAccessoryColorG.Clone();
        clone.HeadAccessoryColorB = (string[]) HeadAccessoryColorB.Clone();
        clone.HeadMarkingColorR = (string[]) HeadMarkingColorR.Clone();
        clone.HeadMarkingColorG = (string[]) HeadMarkingColorG.Clone();
        clone.HeadMarkingColorB = (string[]) HeadMarkingColorB.Clone();
        clone.BodyMarkingColorR = (string[]) BodyMarkingColorR.Clone();
        clone.BodyMarkingColorG = (string[]) BodyMarkingColorG.Clone();
        clone.BodyMarkingColorB = (string[]) BodyMarkingColorB.Clone();
        clone.TailMarkingColorR = (string[]) TailMarkingColorR.Clone();
        clone.TailMarkingColorG = (string[]) TailMarkingColorG.Clone();
        clone.TailMarkingColorB = (string[]) TailMarkingColorB.Clone();
        clone.EyeColorR = (string[]) EyeColorR.Clone();
        clone.EyeColorG = (string[]) EyeColorG.Clone();
        clone.EyeColorB = (string[]) EyeColorB.Clone();
        clone.Gender = (string[]) Gender.Clone();
        clone.BeardStyle = (string[]) BeardStyle.Clone();
        clone.HairStyle = (string[]) HairStyle.Clone();
        clone.HeadAccessoryStyle = (string[]) HeadAccessoryStyle.Clone();
        clone.HeadMarkingStyle = (string[]) HeadMarkingStyle.Clone();
        clone.BodyMarkingStyle = (string[]) BodyMarkingStyle.Clone();
        clone.TailMarkingStyle = (string[]) TailMarkingStyle.Clone();

        return clone;
    }
}



