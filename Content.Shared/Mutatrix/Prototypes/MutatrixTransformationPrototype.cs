// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Maths;
using Robust.Shared.Utility;

namespace Content.Shared.Mutatrix.Prototypes;

/// <summary>
/// Defines a species available to the Mutatrix.
///
/// The Mutatrix does not copy scanned mobs. It unlocks this data entry and then
/// uses the configured PolymorphPrototype to transform into a generic mob.
/// </summary>
[Prototype("mutatrixTransformation")]
public sealed partial class MutatrixTransformationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Polymorph prototype used to perform the actual transformation.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<PolymorphPrototype> Polymorph;

    /// <summary>
    /// Generic mob prototype represented by this transformation.
    /// Used for scanning and fallback icon generation.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId MobPrototype;

    /// <summary>
    /// Additional entity prototypes that unlock this species when scanned.
    /// If empty, <see cref="MobPrototype"/> is used.
    /// </summary>
    [DataField]
    public HashSet<EntProtoId> ScanPrototypes = new();

    /// <summary>
    /// Humanoid species IDs that unlock this transformation when scanned.
    /// Use this for player races, because their entity prototype is often a
    /// generic humanoid while the real race is stored on HumanoidAppearanceComponent.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<SpeciesPrototype>> ScanSpecies = new();

    /// <summary>
    /// LocId for the display name.
    /// </summary>
    [DataField]
    public string Name = string.Empty;

    /// <summary>
    /// LocId for the description.
    /// </summary>
    [DataField]
    public string Description = string.Empty;

    /// <summary>
    /// LocId/category shown by the menu.
    /// </summary>
    [DataField]
    public string Category = "mutatrix-category-uncategorized";

    /// <summary>
    /// Optional radial menu icon. If null, the client may fall back to the mob prototype sprite.
    /// </summary>
    [DataField]
    public SpriteSpecifier? Icon;

    /// <summary>
    /// Unlock this DNA when the database is initialized.
    /// </summary>
    [DataField]
    public bool DefaultUnlocked;

    /// <summary>
    /// Whether this entry can be unlocked by scanning nearby mobs.
    /// </summary>
    [DataField]
    public bool CanScan = true;

    /// <summary>
    /// Optional scan duration override in seconds.
    /// </summary>
    [DataField]
    public float? ScanTime;

    /// <summary>
    /// Optional skin/body tint applied after polymorphing into a humanoid species.
    /// This is intentionally lightweight; detailed markings are still configured
    /// by the target mob/species prototype.
    /// </summary>
    [DataField]
    public Color? SkinColor;

    /// <summary>
    /// Optional eye color applied after polymorphing into a humanoid species.
    /// </summary>
    [DataField]
    public Color? EyeColor;

    /// <summary>
    /// Optional secondary/detail color used for markings, screens, fins, tails,
    /// circuits and other species-specific accent layers.
    /// </summary>
    [DataField]
    public Color? DetailColor;

    /// <summary>
    /// Sort order inside the radial menu.
    /// </summary>
    [DataField]
    public int Order;
}
