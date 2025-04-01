using System.Linq;
using System.Threading.Tasks;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.Inventory;
using Content.Server.Prayer;
using Content.Shared.Buckle;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Forensics.Components;
using Content.Shared.Genetics;
using Content.Shared.Genetics.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Genetics.System;

public sealed partial class DnaModifierSystem : SharedDnaModifierSystem
{
    [Dependency] private readonly IAdminLogManager _admin = default!;
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly EnsureMarkingSystem _ensureMarking = default!;
    [Dependency] private readonly StructuralEnzymesIndexerSystem _enzymesIndexer = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ServerInventorySystem _inventory = default!;
    [Dependency] private readonly MarkingPrototypesIndexerSystem _markingIndexer = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PrayerSystem _prayerSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeInjector();
        InitializeMap();

        SubscribeLocalEvent<DnaModifierComponent, ComponentInit>(OnDnaModifierInit);
        SubscribeLocalEvent<DnaModifierDeviationComponent, ComponentStartup>(OnDnaDeviation);

        SubscribeLocalEvent<DnaModifierComponent, CureDnaDiseaseAttemptEvent>(OnTryCureDnaDisease);
        SubscribeLocalEvent<DnaModifierComponent, MutateDnaAttemptEvent>(OnTryMutateDna);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var instabilityQuery = EntityQueryEnumerator<DnaInstabilityComponent>();
        while (instabilityQuery.MoveNext(out var uid, out var instabilityComponent))
        {
            if (instabilityComponent.NextTimeTick <= 0)
            {
                instabilityComponent.NextTimeTick = 10;
                if (!TryComp<MobThresholdsComponent>(uid, out var uidThresholds)
                    || uidThresholds.CurrentThresholdState is MobState.Dead)
                    return;

                switch (instabilityComponent.Stage)
                {
                    case 1: InstabilityStageOne(uid); break;
                    case 2: InstabilityStageTwo(uid); break;
                    case 3: InstabilityStageThree(uid); break;
                    default: break;
                }
            }
            instabilityComponent.NextTimeTick -= frameTime;
        }
    }

    private void OnDnaModifierInit(EntityUid uid, DnaModifierComponent component, ComponentInit args)
    {
        InitializeStructuralEnzymes(uid, component);

        _ = InitializeDelayAsync(uid, component);
    }

    private void OnDnaDeviation(EntityUid uid, DnaModifierDeviationComponent component, ComponentStartup args)
    {
        if (!TryComp<DnaModifierComponent>(uid, out var dnaModifier) || dnaModifier.EnzymesPrototypes == null)
            return;

        var diseaseEnzymes = dnaModifier.EnzymesPrototypes
            .Where(enzyme =>
            {
                if (!_prototype.TryIndex<StructuralEnzymesPrototype>(enzyme.EnzymesPrototypeId, out var enzymePrototype))
                    return false;

                return enzymePrototype.TypeDeviation == EnzymesType.Disease;
            })
            .ToList();

        if (diseaseEnzymes.Count == 0)
            return;

        int countToModify = _random.Next(1, Math.Min(3, diseaseEnzymes.Count + 1));

        var enzymesToModify = diseaseEnzymes
            .OrderBy(_ => _random.Next())
            .Take(countToModify)
            .ToList();

        foreach (var enzyme in enzymesToModify)
        {
            enzyme.HexCode = GetHexCodeDisease();
        }

        TryChangeStructuralEnzymes(dnaModifier);

        Dirty(uid, dnaModifier);
    }

    private async Task InitializeDelayAsync(EntityUid uid, DnaModifierComponent component)
    {
        await Task.Delay(1);
        InitializeUniqueIdentifiers(uid, component);

        await Task.Delay(1);
        CheckDeviations(uid, component);

        Dirty(uid, component);
    }

    #region Deep Cloning
    public UniqueIdentifiersPrototype? CloneUniqueIdentifiers(UniqueIdentifiersPrototype? source)
    {
        if (source == null)
            return null;

        return (UniqueIdentifiersPrototype)source.Clone();
    }

    public List<EnzymesPrototypeInfo>? CloneEnzymesPrototypes(List<EnzymesPrototypeInfo>? source)
    {
        if (source == null)
            return null;

        return source.Select(e => (EnzymesPrototypeInfo)e.Clone()).ToList();
    }
    #endregion

    #region Initialize U.I.
    private void InitializeUniqueIdentifiers(EntityUid uid, DnaModifierComponent component)
    {
        if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
        {
            var uniqueIdentifiers = new UniqueIdentifiersPrototype
            {
                ID = $"UniqueIdentifiers{uid}",
            };

            var markingSet = humanoid.MarkingSet;
            var markingPrototypes = _markingIndexer.GetAllMarkingPrototypes();
            var speciesProto = _prototype.Index<SpeciesPrototype>(humanoid.Species);

            var empty = new[] { "0", "0", "0" };

            // Цвет волос (блоки 1-3) и Вторичный цвет волос (блоки 4-6)
            if (markingSet.TryGetCategory(MarkingCategories.Hair, out var hairMarkings))
            {
                // блоки 1-3
                var hairColor = GetFirstMarkingColor(hairMarkings);
                var hairColorArray = ConvertColorToHexArray(hairColor);
                uniqueIdentifiers.HairColorR = new[] { hairColorArray[0], hairColorArray[1], hairColorArray[2] };
                uniqueIdentifiers.HairColorG = new[] { hairColorArray[3], hairColorArray[4], hairColorArray[5] };
                uniqueIdentifiers.HairColorB = new[] { hairColorArray[6], hairColorArray[7], hairColorArray[8] };

                // блок 34
                var markingId = hairMarkings.FirstOrDefault()?.MarkingId;
                var markingPrototype = markingPrototypes
                    .FirstOrDefault(m => m.MarkingPrototypeId == markingId);

                uniqueIdentifiers.HairStyle = markingPrototype != null
                    ? markingPrototype.HexValue
                    : empty;

                // блоки 4-6
                if (hairMarkings.Count > 1)
                {
                    var secondaryHairColor = hairMarkings[1].MarkingColors.Count > 0
                        ? hairMarkings[1].MarkingColors[0]
                        : Color.White;
                    var secondaryHairColorArray = ConvertColorToHexArray(secondaryHairColor);
                    uniqueIdentifiers.SecondaryHairColorR = new[] { secondaryHairColorArray[0], secondaryHairColorArray[1], secondaryHairColorArray[2] };
                    uniqueIdentifiers.SecondaryHairColorG = new[] { secondaryHairColorArray[3], secondaryHairColorArray[4], secondaryHairColorArray[5] };
                    uniqueIdentifiers.SecondaryHairColorB = new[] { secondaryHairColorArray[6], secondaryHairColorArray[7], secondaryHairColorArray[8] };
                }
                else
                {
                    uniqueIdentifiers.SecondaryHairColorR = GenerateRandomHexValues();
                    uniqueIdentifiers.SecondaryHairColorG = GenerateRandomHexValues();
                    uniqueIdentifiers.SecondaryHairColorB = GenerateRandomHexValues();
                }
            }
            else
            {
                // блоки 1-3
                uniqueIdentifiers.HairColorR = GenerateRandomHexValues();
                uniqueIdentifiers.HairColorG = GenerateRandomHexValues();
                uniqueIdentifiers.HairColorB = GenerateRandomHexValues();
                // блоки 4-6
                uniqueIdentifiers.SecondaryHairColorR = GenerateRandomHexValues();
                uniqueIdentifiers.SecondaryHairColorG = GenerateRandomHexValues();
                uniqueIdentifiers.SecondaryHairColorB = GenerateRandomHexValues();

                // блок 34
                uniqueIdentifiers.HairStyle = empty;
            }

            // Цвет бороды (блоки 7-9)
            if (markingSet.TryGetCategory(MarkingCategories.FacialHair, out var facialHairMarkings))
            {
                var facialHairColor = GetFirstMarkingColor(facialHairMarkings);
                var facialHairColorArray = ConvertColorToHexArray(facialHairColor);
                uniqueIdentifiers.BeardColorR = new[] { facialHairColorArray[0], facialHairColorArray[1], facialHairColorArray[2] };
                uniqueIdentifiers.BeardColorG = new[] { facialHairColorArray[3], facialHairColorArray[4], facialHairColorArray[5] };
                uniqueIdentifiers.BeardColorB = new[] { facialHairColorArray[6], facialHairColorArray[7], facialHairColorArray[8] };

                // блок 33
                var markingId = facialHairMarkings.FirstOrDefault()?.MarkingId;
                var markingPrototype = markingPrototypes
                    .FirstOrDefault(m => m.MarkingPrototypeId == markingId);

                uniqueIdentifiers.BeardStyle = markingPrototype != null
                    ? markingPrototype.HexValue
                    : empty;
            }
            else
            {
                uniqueIdentifiers.BeardColorR = GenerateRandomHexValues();
                uniqueIdentifiers.BeardColorG = GenerateRandomHexValues();
                uniqueIdentifiers.BeardColorB = GenerateRandomHexValues();

                // блок 33
                uniqueIdentifiers.BeardStyle = empty;
            }

            // Тон кожи или цвет меха (блоки 13-16)
            switch (speciesProto.SkinColoration)
            {
                case HumanoidSkinColor.HumanToned:
                    // Для HumanToned и TintedHues заполняем блок 13 (тон кожи)
                    uniqueIdentifiers.SkinTone = ConvertSkinToneToHexArray(humanoid.SkinColor);
                    break;

                case HumanoidSkinColor.Hues:
                case HumanoidSkinColor.TintedHues:
                case HumanoidSkinColor.VoxFeathers:
                    // Для Hues и VoxFeathers заполняем блоки 14-16 (цвет меха)
                    var furColorArray = ConvertColorToHexArray(humanoid.SkinColor);
                    uniqueIdentifiers.FurColorR = new[] { furColorArray[0], furColorArray[1], furColorArray[2] };
                    uniqueIdentifiers.FurColorG = new[] { furColorArray[3], furColorArray[4], furColorArray[5] };
                    uniqueIdentifiers.FurColorB = new[] { furColorArray[6], furColorArray[7], furColorArray[8] };
                    break;
            }

            // Цвет головного аксессуара (блоки 17-19)
            if (markingSet.TryGetCategory(MarkingCategories.HeadTop, out var headTopMarkings))
            {
                var headTopColor = GetFirstMarkingColor(headTopMarkings);
                var headTopColorArray = ConvertColorToHexArray(headTopColor);
                uniqueIdentifiers.HeadAccessoryColorR = new[] { headTopColorArray[0], headTopColorArray[1], headTopColorArray[2] };
                uniqueIdentifiers.HeadAccessoryColorG = new[] { headTopColorArray[3], headTopColorArray[4], headTopColorArray[5] };
                uniqueIdentifiers.HeadAccessoryColorB = new[] { headTopColorArray[6], headTopColorArray[7], headTopColorArray[8] };

                // блок 35
                var markingId = headTopMarkings.FirstOrDefault()?.MarkingId;
                var markingPrototype = markingPrototypes
                    .FirstOrDefault(m => m.MarkingPrototypeId == markingId);

                uniqueIdentifiers.HeadAccessoryStyle = markingPrototype != null
                    ? markingPrototype.HexValue
                    : empty;
            }
            else
            {
                uniqueIdentifiers.HeadAccessoryColorR = GenerateRandomHexValues();
                uniqueIdentifiers.HeadAccessoryColorG = GenerateRandomHexValues();
                uniqueIdentifiers.HeadAccessoryColorB = GenerateRandomHexValues();

                // блок 35
                uniqueIdentifiers.HeadAccessoryStyle = empty;
            }

            // Цвет разметки головы (блоки 20-22)
            if (markingSet.TryGetCategory(MarkingCategories.Head, out var headMarkings))
            {
                var headColor = GetFirstMarkingColor(headMarkings);
                var headColorArray = ConvertColorToHexArray(headColor);
                uniqueIdentifiers.HeadMarkingColorR = new[] { headColorArray[0], headColorArray[1], headColorArray[2] };
                uniqueIdentifiers.HeadMarkingColorG = new[] { headColorArray[3], headColorArray[4], headColorArray[5] };
                uniqueIdentifiers.HeadMarkingColorB = new[] { headColorArray[6], headColorArray[7], headColorArray[8] };

                // блок 36
                var markingId = headMarkings.FirstOrDefault()?.MarkingId;
                var markingPrototype = markingPrototypes
                    .FirstOrDefault(m => m.MarkingPrototypeId == markingId);

                uniqueIdentifiers.HeadMarkingStyle = markingPrototype != null
                    ? markingPrototype.HexValue
                    : empty;
            }
            else
            {
                uniqueIdentifiers.HeadMarkingColorR = GenerateRandomHexValues();
                uniqueIdentifiers.HeadMarkingColorG = GenerateRandomHexValues();
                uniqueIdentifiers.HeadMarkingColorB = GenerateRandomHexValues();

                // блок 36
                uniqueIdentifiers.HeadMarkingStyle = empty;
            }

            // Цвет маркировки тела (блоки 23-25)
            if (markingSet.TryGetCategory(MarkingCategories.Chest, out var chestMarkings))
            {
                var chestColor = GetFirstMarkingColor(chestMarkings);
                var chestColorArray = ConvertColorToHexArray(chestColor);
                uniqueIdentifiers.BodyMarkingColorR = new[] { chestColorArray[0], chestColorArray[1], chestColorArray[2] };
                uniqueIdentifiers.BodyMarkingColorG = new[] { chestColorArray[3], chestColorArray[4], chestColorArray[5] };
                uniqueIdentifiers.BodyMarkingColorB = new[] { chestColorArray[6], chestColorArray[7], chestColorArray[8] };

                // блок 37
                var markingId = chestMarkings.FirstOrDefault()?.MarkingId;
                var markingPrototype = markingPrototypes
                    .FirstOrDefault(m => m.MarkingPrototypeId == markingId);

                uniqueIdentifiers.BodyMarkingStyle = markingPrototype != null
                    ? markingPrototype.HexValue
                    : empty;
            }
            else
            {
                uniqueIdentifiers.BodyMarkingColorR = GenerateRandomHexValues();
                uniqueIdentifiers.BodyMarkingColorG = GenerateRandomHexValues();
                uniqueIdentifiers.BodyMarkingColorB = GenerateRandomHexValues();

                // блок 37
                uniqueIdentifiers.BodyMarkingStyle = empty;
            }

            // Цвет маркировки хвоста (блоки 26-28)
            if (markingSet.TryGetCategory(MarkingCategories.Tail, out var tailMarkings))
            {
                var tailColor = GetFirstMarkingColor(tailMarkings);
                var tailColorArray = ConvertColorToHexArray(tailColor);
                uniqueIdentifiers.TailMarkingColorR = new[] { tailColorArray[0], tailColorArray[1], tailColorArray[2] };
                uniqueIdentifiers.TailMarkingColorG = new[] { tailColorArray[3], tailColorArray[4], tailColorArray[5] };
                uniqueIdentifiers.TailMarkingColorB = new[] { tailColorArray[6], tailColorArray[7], tailColorArray[8] };

                // блок 38
                var markingId = tailMarkings.FirstOrDefault()?.MarkingId;
                var markingPrototype = markingPrototypes
                    .FirstOrDefault(m => m.MarkingPrototypeId == markingId);

                uniqueIdentifiers.TailMarkingStyle = markingPrototype != null
                    ? markingPrototype.HexValue
                    : empty;
            }
            else
            {
                uniqueIdentifiers.TailMarkingColorR = GenerateRandomHexValues();
                uniqueIdentifiers.TailMarkingColorG = GenerateRandomHexValues();
                uniqueIdentifiers.TailMarkingColorB = GenerateRandomHexValues();

                // блок 38
                uniqueIdentifiers.TailMarkingStyle = empty;
            }

            // Цвет глаз (блоки 29-31)
            var eyeColorArray = ConvertColorToHexArray(humanoid.EyeColor);
            uniqueIdentifiers.EyeColorR = new[] { eyeColorArray[0], eyeColorArray[1], eyeColorArray[2] };
            uniqueIdentifiers.EyeColorG = new[] { eyeColorArray[3], eyeColorArray[4], eyeColorArray[5] };
            uniqueIdentifiers.EyeColorB = new[] { eyeColorArray[6], eyeColorArray[7], eyeColorArray[8] };

            // Пол (блок 32)
            uniqueIdentifiers.Gender = humanoid.Sex switch
            {
                Sex.Female => GenerateTripleHexValues(0x0, 0x5, 0x0, 0x7, 0x0, 0x3), // <= 0x5 <= 0x7 <= 0x3
                Sex.Male => GenerateTripleHexValues(0x0, 0x7, 0x0, 0x7, 0x0, 0x8), // < 0x8 <= 0x7 < 0x9
                Sex.Unsexed => GenerateTripleHexValues(0x8, 0xF, 0x7, 0xF, 0x9, 0xF), // >= 0x8 >= 0x7 >= 0x9
                _ => GenerateRandomHexValues()
            };

            component.UniqueIdentifiers = uniqueIdentifiers;
        }
        else
        {
            var empty = new[] { "0", "0", "0" };
            var uniqueIdentifiers = new UniqueIdentifiersPrototype
            {
                ID = $"StructuralEnzymes{uid}",
                HairColorR = GenerateRandomHexValues(),
                HairColorG = GenerateRandomHexValues(),
                HairColorB = GenerateRandomHexValues(),
                SecondaryHairColorR = GenerateRandomHexValues(),
                SecondaryHairColorG = GenerateRandomHexValues(),
                SecondaryHairColorB = GenerateRandomHexValues(),
                BeardColorR = GenerateRandomHexValues(),
                BeardColorG = GenerateRandomHexValues(),
                BeardColorB = GenerateRandomHexValues(),
                SkinTone = GenerateRandomToneValues(),
                FurColorR = GenerateRandomHexValues(),
                FurColorG = GenerateRandomHexValues(),
                FurColorB = GenerateRandomHexValues(),
                HeadAccessoryColorR = GenerateRandomHexValues(),
                HeadAccessoryColorG = GenerateRandomHexValues(),
                HeadAccessoryColorB = GenerateRandomHexValues(),
                HeadMarkingColorR = GenerateRandomHexValues(),
                HeadMarkingColorG = GenerateRandomHexValues(),
                HeadMarkingColorB = GenerateRandomHexValues(),
                BodyMarkingColorR = GenerateRandomHexValues(),
                BodyMarkingColorG = GenerateRandomHexValues(),
                BodyMarkingColorB = GenerateRandomHexValues(),
                TailMarkingColorR = GenerateRandomHexValues(),
                TailMarkingColorG = GenerateRandomHexValues(),
                TailMarkingColorB = GenerateRandomHexValues(),
                EyeColorR = GenerateRandomHexValues(),
                EyeColorG = GenerateRandomHexValues(),
                EyeColorB = GenerateRandomHexValues(),
                Gender = _random.Next(0, 2) == 0
                    ? GenerateRandomGenderHexValue(0x000, 0x23D) // Женщина
                    : GenerateRandomGenderHexValue(0x23E, 0x320), // Мужчина
                HairStyle = GenerateRandomHexValues(),
                BeardStyle = GenerateRandomHexValues(),
                HeadAccessoryStyle = empty,
                HeadMarkingStyle = empty,
                BodyMarkingStyle = empty,
                TailMarkingStyle = empty
            };

            component.UniqueIdentifiers = uniqueIdentifiers;
        }
    }
    #endregion

    #region Initialize S.E.
    private void InitializeStructuralEnzymes(EntityUid uid, DnaModifierComponent component)
    {
        var enzymesPrototypes = _enzymesIndexer.GetAllEnzymesPrototypes();
        var uniqueEnzymesPrototypes = new List<EnzymesPrototypeInfo>();
        bool hasHumanoidAppearance = HasComp<HumanoidAppearanceComponent>(uid);
        foreach (var enzymePrototype in enzymesPrototypes)
        {
            var uniqueEnzyme = new EnzymesPrototypeInfo
            {
                EnzymesPrototypeId = enzymePrototype.EnzymesPrototypeId,
                Order = enzymePrototype.Order,
                HexCode = enzymePrototype.Order == 55
                    ? (hasHumanoidAppearance ? GenerateLastHexCode() : GenerateHexCode())
                    : GenerateHexCode()
            };

            uniqueEnzymesPrototypes.Add(uniqueEnzyme);
        }

        component.EnzymesPrototypes = uniqueEnzymesPrototypes;
    }

    private string[] GenerateHexCode()
    {
        var firstDigit = _random.Next(0, 3).ToString("X1");
        var secondDigit = _random.Next(0, 16).ToString("X1");
        var thirdDigit = _random.Next(0, 16).ToString("X1");

        return new[] { firstDigit, secondDigit, thirdDigit };
    }

    private string[] GenerateLastHexCode()
    {
        var firstDigit = _random.Next(8, 16).ToString("X1");
        var secondDigit = _random.Next(0, 16).ToString("X1");
        var thirdDigit = _random.Next(0, 16).ToString("X1");

        return new[] { firstDigit, secondDigit, thirdDigit };
    }
    #endregion

    #region Instability
    private void UpdateInstability(EntityUid uid, DnaModifierComponent component, int totalInstability)
    {
        component.Instability = totalInstability;
        if (totalInstability <= 20)
        {
            if (HasComp<DnaInstabilityComponent>(uid))
                RemComp<DnaInstabilityComponent>(uid);
            return;
        }

        var instabilityComp = EnsureComp<DnaInstabilityComponent>(uid);
        switch (totalInstability)
        {
            case > 20 and <= 35:
                instabilityComp.Stage = 1;
                break;

            case > 35 and <= 65:
                instabilityComp.Stage = 2;
                break;

            case > 65:
                instabilityComp.Stage = 3;
                break;
        }

        Dirty(uid, component);
    }

    private void CheckDeviations(EntityUid uid, DnaModifierComponent component)
    {
        if (component.EnzymesPrototypes == null)
            return;

        int totalInstability = component.Instability;
        foreach (var enzyme in component.EnzymesPrototypes)
        {
            if (!_prototype.TryIndex<StructuralEnzymesPrototype>(enzyme.EnzymesPrototypeId, out var enzymePrototype))
                continue;

            bool hasComponent = enzymePrototype.AddComponent != null && enzymePrototype.AddComponent
                .Any(componentEntry =>
                {
                    var componentType = componentEntry.Value.Component?.GetType();
                    return componentType != null && HasComp(uid, componentType);
                });

            if (hasComponent && enzymePrototype.TypeDeviation == EnzymesType.Disease)
            {
                enzyme.HexCode = GetHexCodeDisease();
                totalInstability += enzymePrototype.CostInstability;
            }
        }

        UpdateInstability(uid, component, totalInstability);
    }

    private string[] GetHexCodeDisease()
    {
        return new[]
        {
            _random.Next(9, 16).ToString("X1"),
            _random.Next(0, 16).ToString("X1"),
            _random.Next(2, 16).ToString("X1")
        };
    }

    private void InstabilityStageOne(EntityUid uid)
    {
        if (_random.NextFloat() < 0.05f)
        {
            var damage = new DamageSpecifier { DamageDict = { { "Heat", 2.5 } } };
            _damage.TryChangeDamage(uid, damage, true);

            _popup.PopupEntity(Loc.GetString("dna-instability-stage-one"), uid, uid, PopupType.SmallCaution);
        }
    }

    private void InstabilityStageTwo(EntityUid uid)
    {
        if (_random.NextFloat() < 0.25f)
        {
            var heatDamage = new DamageSpecifier { DamageDict = { { "Heat", 2.5 } } };
            var bluntDamage = new DamageSpecifier { DamageDict = { { "Blunt", 10 } } };
            var structuralDamage = new DamageSpecifier { DamageDict = { { "Structural", 2 } } };

            _damage.TryChangeDamage(uid, heatDamage, true);
            _damage.TryChangeDamage(uid, bluntDamage, true);
            _damage.TryChangeDamage(uid, structuralDamage, true);

            _chat.TryEmoteWithoutChat(uid, _prototype.Index<EmotePrototype>("Scream"), true);
            _popup.PopupEntity(Loc.GetString("dna-instability-stage-two"), uid, uid, PopupType.SmallCaution);
        }
    }

    private void InstabilityStageThree(EntityUid uid)
    {
        if (_random.NextFloat() < 0.5f)
        {
            var heatDamage = new DamageSpecifier { DamageDict = { { "Heat", 5 } } };
            var bluntDamage = new DamageSpecifier { DamageDict = { { "Blunt", 50 } } };
            var structuralDamage = new DamageSpecifier { DamageDict = { { "Structural", 4 } } };

            _damage.TryChangeDamage(uid, heatDamage, true);
            _damage.TryChangeDamage(uid, bluntDamage, true);
            _damage.TryChangeDamage(uid, structuralDamage, true);

            _chat.TryEmoteWithoutChat(uid, _prototype.Index<EmotePrototype>("Scream"), true);
            _popup.PopupEntity(Loc.GetString("dna-instability-stage-three"), uid, uid, PopupType.LargeCaution);
        }
    }
    #endregion

    #region Modify U.I.
    public void ChangeDna(DnaModifierComponent component, EnzymeInfo enzyme)
    {
        if (enzyme.Identifier != null) component.UniqueIdentifiers = enzyme.Identifier;
        if (enzyme.Info != null) component.EnzymesPrototypes = enzyme.Info;

        Dirty(component.Owner, component);

        TryChangeUniqueIdentifiers(component);
        TryChangeStructuralEnzymes(component);
    }

    public void ChangeDna(DnaModifierComponent component, int type)
    {
        if (type == 0)
        {
            TryChangeUniqueIdentifiers(component);
        }
        else if (type == 1)
        {
            TryChangeStructuralEnzymes(component);
        }
    }

    public void ChangeDna(DnaModifierComponent component)
    {
        TryChangeUniqueIdentifiers(component);
        TryChangeStructuralEnzymes(component);
    }

    private void TryChangeUniqueIdentifiers(DnaModifierComponent component)
    {
        if (!TryComp<HumanoidAppearanceComponent>(component.Owner, out var humanoid) || component.UniqueIdentifiers == null)
            return;

        var uniqueIdentifiers = component.UniqueIdentifiers;
        UpdateSkin(humanoid, uniqueIdentifiers);
        UpdateMarkings(humanoid, uniqueIdentifiers);
        UpdateEyeColor(humanoid, uniqueIdentifiers);
        UpdateGender(humanoid, uniqueIdentifiers);

        Dirty(humanoid.Owner, humanoid);
    }

    private void UpdateSkin(HumanoidAppearanceComponent humanoid, UniqueIdentifiersPrototype uniqueIdentifiers)
    {
        var speciesProto = _prototype.Index<SpeciesPrototype>(humanoid.Species);

        switch (speciesProto.SkinColoration)
        {
            case HumanoidSkinColor.HumanToned:
                humanoid.SkinColor = ConvertSkinToneToColor(uniqueIdentifiers.SkinTone);
                break;

            case HumanoidSkinColor.Hues:
            case HumanoidSkinColor.TintedHues:
            case HumanoidSkinColor.VoxFeathers:
                string redHex = uniqueIdentifiers.FurColorR[0] + uniqueIdentifiers.FurColorR[1];
                string greenHex = uniqueIdentifiers.FurColorG[0] + uniqueIdentifiers.FurColorG[1];
                string blueHex = uniqueIdentifiers.FurColorB[0] + uniqueIdentifiers.FurColorB[1];

                int red = Convert.ToInt32(redHex, 16);
                int green = Convert.ToInt32(greenHex, 16);
                int blue = Convert.ToInt32(blueHex, 16);

                float redNormalized = red / 255f;
                float greenNormalized = green / 255f;
                float blueNormalized = blue / 255f;

                var newColor = new Color(redNormalized, greenNormalized, blueNormalized);

                humanoid.SkinColor = newColor;
                break;
        }
    }

    private void UpdateMarkings(HumanoidAppearanceComponent humanoid, UniqueIdentifiersPrototype uniqueIdentifiers)
    {
        var markingSet = humanoid.MarkingSet;
        var markingPrototypes = _markingIndexer.GetAllMarkingPrototypes();

        var target = humanoid.Owner;
        _ensureMarking.UpdateMarkingCategory(target, markingSet, MarkingCategories.Hair, uniqueIdentifiers.HairColorR, uniqueIdentifiers.HairColorG, uniqueIdentifiers.HairColorB, uniqueIdentifiers.HairStyle, humanoid.Species, markingPrototypes);
        _ensureMarking.UpdateMarkingCategory(target, markingSet, MarkingCategories.FacialHair, uniqueIdentifiers.BeardColorR, uniqueIdentifiers.BeardColorG, uniqueIdentifiers.BeardColorB, uniqueIdentifiers.BeardStyle, humanoid.Species, markingPrototypes);
        _ensureMarking.UpdateMarkingCategory(target, markingSet, MarkingCategories.HeadTop, uniqueIdentifiers.HeadAccessoryColorR, uniqueIdentifiers.HeadAccessoryColorG, uniqueIdentifiers.HeadAccessoryColorB, uniqueIdentifiers.HeadAccessoryStyle, humanoid.Species, markingPrototypes);
        _ensureMarking.UpdateMarkingCategory(target, markingSet, MarkingCategories.Head, uniqueIdentifiers.HeadMarkingColorR, uniqueIdentifiers.HeadMarkingColorG, uniqueIdentifiers.HeadMarkingColorB, uniqueIdentifiers.HeadMarkingStyle, humanoid.Species, markingPrototypes);
        _ensureMarking.UpdateMarkingCategory(target, markingSet, MarkingCategories.Chest, uniqueIdentifiers.BodyMarkingColorR, uniqueIdentifiers.BodyMarkingColorG, uniqueIdentifiers.BodyMarkingColorB, uniqueIdentifiers.BodyMarkingStyle, humanoid.Species, markingPrototypes);
        _ensureMarking.UpdateMarkingCategory(target, markingSet, MarkingCategories.Tail, uniqueIdentifiers.TailMarkingColorR, uniqueIdentifiers.TailMarkingColorG, uniqueIdentifiers.TailMarkingColorB, uniqueIdentifiers.TailMarkingStyle, humanoid.Species, markingPrototypes);
    }

    private void UpdateEyeColor(HumanoidAppearanceComponent humanoid, UniqueIdentifiersPrototype uniqueIdentifiers)
    {
        string redHex = uniqueIdentifiers.EyeColorR[0] + uniqueIdentifiers.EyeColorR[1];
        string greenHex = uniqueIdentifiers.EyeColorG[0] + uniqueIdentifiers.EyeColorG[1];
        string blueHex = uniqueIdentifiers.EyeColorB[0] + uniqueIdentifiers.EyeColorB[1];

        int red = Convert.ToInt32(redHex, 16);
        int green = Convert.ToInt32(greenHex, 16);
        int blue = Convert.ToInt32(blueHex, 16);

        float redNormalized = red / 255f;
        float greenNormalized = green / 255f;
        float blueNormalized = blue / 255f;

        var eyeColor = new Color(redNormalized, greenNormalized, blueNormalized);

        humanoid.EyeColor = eyeColor;
    }

    private void UpdateGender(HumanoidAppearanceComponent humanoid, UniqueIdentifiersPrototype uniqueIdentifiers)
    {
        int[] values = uniqueIdentifiers.Gender
            .Select(hex => Convert.ToInt32(hex, 16))
            .ToArray();

        var currentGender = (values[0], values[1], values[2]) switch
        {
            ( <= 0x5, <= 0x7, <= 0x3) => Gender.Female,
            ( < 0x8, <= 0x7, < 0x9) => Gender.Male,
            ( >= 0x8, >= 0x7, >= 0x9) => Gender.Neuter,
            _ => Gender.Neuter
        };

        var currentSex = (values[0], values[1], values[2]) switch
        {
            ( <= 0x5, <= 0x7, <= 0x3) => Sex.Female,
            ( < 0x8, <= 0x7, < 0x9) => Sex.Male,
            ( >= 0x8, >= 0x7, >= 0x9) => Sex.Unsexed,
            _ => Sex.Unsexed
        };

        humanoid.Gender = currentGender;
        humanoid.Sex = currentSex;
    }
    #endregion Modify U.I.

    #region Modify S.E.
    private void TryChangeStructuralEnzymes(DnaModifierComponent component)
    {
        if (component.EnzymesPrototypes == null)
            return;

        var target = component.Owner;
        int totalInstability = component.Instability;
        var enzymes = component.EnzymesPrototypes;
        var messagesToShow = new List<string>();
        foreach (var enzyme in enzymes)
        {
            if (enzyme.Order == 55)
            {
                TryChangeLastBlock(target, component, enzyme);
                continue;
            }

            if (!_prototype.TryIndex<StructuralEnzymesPrototype>(enzyme.EnzymesPrototypeId, out var enzymePrototype))
                continue;

            bool meetsCondition = CheckHexCodeCondition(enzyme.HexCode, enzymePrototype.TypeDeviation);
            if (enzymePrototype.AddComponent != null)
            {
                if (meetsCondition)
                {
                    bool hasAnyComponent = enzymePrototype.AddComponent
                        .Any(componentEntry =>
                        {
                            var componentType = componentEntry.Value.Component?.GetType();
                            return componentType != null && HasComp(target, componentType);
                        });

                    if (!hasAnyComponent && _random.NextFloat() <= enzymePrototype.ChanceAssimilation)
                    {
                        EntityManager.AddComponents(target, enzymePrototype.AddComponent, false);
                        totalInstability += enzymePrototype.CostInstability;

                        if (!string.IsNullOrEmpty(enzymePrototype.Message))
                            messagesToShow.Add(enzymePrototype.Message);

                        _admin.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(target):user} acquires a gene type: '{enzymePrototype.ID}'.");
                    }
                }
                else
                {
                    foreach (var componentEntry in enzymePrototype.AddComponent)
                    {
                        var componentType = componentEntry.Value.Component?.GetType();
                        if (componentType != null && HasComp(target, componentType))
                        {
                            RemComp(target, componentType);
                            totalInstability -= enzymePrototype.CostInstability;

                            _admin.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(target):user} loses the gene type: '{enzymePrototype.ID}'.");
                        }
                    }
                }
            }
        }

        UpdateInstability(target, component, totalInstability);
        if (messagesToShow.Count > 0)
        {
            _ = ShowMessagesWithDelay(target, messagesToShow);
        }
    }

    private void TryChangeLastBlock(EntityUid target, DnaModifierComponent component, EnzymesPrototypeInfo enzyme)
    {
        if (string.IsNullOrEmpty(component.Upper) || string.IsNullOrEmpty(component.Lowest))
            return;

        int hexValue = Convert.ToInt32(enzyme.HexCode[0], 16);
        if (hexValue < 8)
        {
            if (!HasComp<HumanoidAppearanceComponent>(target))
                return;

            // Zero add an entity
            _buckle.TryUnbuckle(target, target, true);
            var child = _entManager.SpawnEntity(component.Lowest, Transform(target).Coordinates);
            if (TryComp<DamageableComponent>(child, out var damageParent)
                && _mobThreshold.GetScaledDamage(target, child, out var damage) && damage != null)
            {
                _damage.SetDamage(child, damageParent, damage);
            }

            EnsureComp<DnaLowestComponent>(child).Parent = target;

            // First undress
            if (_inventory.TryGetContainerSlotEnumerator(target, out var enumerator))
            {
                while (enumerator.MoveNext(out var slot))
                {
                    _inventory.TryUnequip(target, slot.ID, true, true);
                }
            }

            foreach (var held in _hands.EnumerateHeld(target))
            {
                _hands.TryDrop(target, held);
            }

            // Second customization
            if (TryComp(target, out MetaDataComponent? targetMeta))
                _metaData.SetEntityName(child, targetMeta.EntityName);

            if (_mindSystem.TryGetMind(target, out var mindId, out var mind))
                _mindSystem.TransferTo(mindId, child, mind: mind);

            if (TryComp(target, out DnaComponent? targetDna))
                EnsureComp<DnaComponent>(child).DNA = targetDna.DNA;

            var childDnaModifier = EnsureComp<DnaModifierComponent>(child);
            childDnaModifier.UniqueIdentifiers = component.UniqueIdentifiers;
            childDnaModifier.EnzymesPrototypes = component.EnzymesPrototypes?.ToList();
            childDnaModifier.Instability = component.Instability;
            childDnaModifier.Upper = component.Upper;
            childDnaModifier.Lowest = component.Lowest;

            Dirty(child, childDnaModifier);
            ChangeDna(childDnaModifier);

            _admin.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(target):user} gene down up a step.");

            // Third clearing
            EnsurePausedMap();
            if (PausedMap != null)
            {
                _transform.SetParent(target, Transform(target), PausedMap.Value);
            }
        }
        else
        {
            if (HasComp<HumanoidAppearanceComponent>(target))
                return;

            // Minus one check parent
            if (TryComp<DnaLowestComponent>(target, out var dnaLowest) && dnaLowest.Parent != null)
            {
                var parent = dnaLowest.Parent.Value;
                if (_inventory.TryGetContainerSlotEnumerator(target, out var enumeratorLowest))
                {
                    while (enumeratorLowest.MoveNext(out var slot))
                    {
                        _inventory.TryUnequip(target, slot.ID, true, true);
                    }
                }

                foreach (var held in _hands.EnumerateHeld(target))
                {
                    _hands.TryDrop(target, held);
                }

                foreach (var held in _hands.EnumerateHeld(target))
                {
                    _hands.TryDrop(target, held);
                    _hands.TryPickupAnyHand(parent, held, checkActionBlocker: false);
                }

                if (_mindSystem.TryGetMind(target, out var mindIdLowest, out var mindLowest))
                    _mindSystem.TransferTo(mindIdLowest, parent, mind: mindLowest);

                if (TryComp<DamageableComponent>(parent, out var parentDamage)
                    && _mobThreshold.GetScaledDamage(target, parent, out var damageLowest) && damageLowest != null)
                {
                    _damage.SetDamage(parent, parentDamage, damageLowest);
                }

                if (TryComp<DnaModifierComponent>(parent, out var dnaModifier))
                {
                    dnaModifier.UniqueIdentifiers = component.UniqueIdentifiers;
                    dnaModifier.EnzymesPrototypes = component.EnzymesPrototypes?.ToList();
                    dnaModifier.Instability = component.Instability;
                    dnaModifier.Upper = component.Upper;
                    dnaModifier.Lowest = component.Lowest;

                    Dirty(parent, dnaModifier);
                    ChangeDna(dnaModifier);
                }

                var parentXform = Transform(parent);
                _transform.SetCoordinates(parent, parentXform, Transform(target).Coordinates);
                _transform.AttachToGridOrMap(parent, parentXform);

                _entManager.DeleteEntity(target);
                return;
            }

            // Zero add an entity
            _buckle.TryUnbuckle(target, target, true);
            var child = _entManager.SpawnEntity(component.Upper, Transform(target).Coordinates);
            if (TryComp<DamageableComponent>(child, out var damageParent)
                && _mobThreshold.GetScaledDamage(target, child, out var damage) && damage != null)
            {
                _damage.SetDamage(child, damageParent, damage);
            }

            // First undress
            if (_inventory.TryGetContainerSlotEnumerator(target, out var enumerator))
            {
                while (enumerator.MoveNext(out var slot))
                {
                    _inventory.TryUnequip(target, slot.ID, true, true);
                }
            }

            foreach (var held in _hands.EnumerateHeld(target))
            {
                _hands.TryDrop(target, held);
            }

            // Second customization
            if (TryComp(target, out MetaDataComponent? targetMeta))
                _metaData.SetEntityName(child, targetMeta.EntityName);

            if (_mindSystem.TryGetMind(target, out var mindId, out var mind))
                _mindSystem.TransferTo(mindId, child, mind: mind);

            if (TryComp(target, out DnaComponent? targetDna))
                EnsureComp<DnaComponent>(child).DNA = targetDna.DNA;

            var childDnaModifier = EnsureComp<DnaModifierComponent>(child);
            childDnaModifier.UniqueIdentifiers = component.UniqueIdentifiers;
            childDnaModifier.EnzymesPrototypes = component.EnzymesPrototypes?.ToList();
            childDnaModifier.Instability = component.Instability;
            childDnaModifier.Upper = component.Upper;
            childDnaModifier.Lowest = component.Lowest;

            Dirty(child, childDnaModifier);
            ChangeDna(childDnaModifier);

            _admin.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(target):user} gene went up a step.");

            // Third clearing
            _entManager.DeleteEntity(target); // Bye
        }
    }

    private async Task ShowMessagesWithDelay(EntityUid target, List<string> messages)
    {
        if (!TryComp<ActorComponent>(target, out var actor))
            return;

        foreach (var message in messages)
        {
            _prayerSystem.SendSubtleMessage(actor.PlayerSession, actor.PlayerSession, string.Empty, Loc.GetString(message));
            await Task.Delay(2000);
        }
    }

    private bool CheckHexCodeCondition(string[] hexCode, EnzymesType type)
    {
        int[] values = hexCode.Select(hex => Convert.ToInt32(hex, 16)).ToArray();

        switch (type)
        {
            case EnzymesType.Disease:
            case EnzymesType.Minor:
                return values[0] > 8 || (values[0] == 8 && values[1] >= 0 && values[2] >= 2);

            case EnzymesType.Intermediate:
                return values[0] > 0xB || (values[0] == 0xB && values[1] >= 0xE && values[2] >= 0xA);

            case EnzymesType.Base:
                return values[0] > 0xD || (values[0] == 0xD && values[1] >= 0xA && values[2] >= 0xC);

            default: return false;
        }
    }
    #endregion Modify S.E.

    #region Chemistry
    private void OnTryCureDnaDisease(EntityUid uid, DnaModifierComponent component, CureDnaDiseaseAttemptEvent args)
    {
        if (component.EnzymesPrototypes == null)
            return;

        foreach (var enzyme in component.EnzymesPrototypes)
        {
            if (!_prototype.TryIndex<StructuralEnzymesPrototype>(enzyme.EnzymesPrototypeId, out var enzymePrototype))
                continue;

            if (enzymePrototype.TypeDeviation == EnzymesType.Disease)
            {
                int[] values = enzyme.HexCode.Select(hex => Convert.ToInt32(hex, 16)).ToArray();
                if (values[0] >= 8 && values[1] >= 0 && values[2] >= 2)
                {
                    enzyme.HexCode = GenerateHexCode();
                }
            }
        }

        TryChangeStructuralEnzymes(component);

        Dirty(uid, component);
    }

    private void OnTryMutateDna(EntityUid uid, DnaModifierComponent component, MutateDnaAttemptEvent args)
    {
        if (component.EnzymesPrototypes == null)
            return;

        foreach (var enzyme in component.EnzymesPrototypes)
        {
            if (enzyme.Order == 55)
            {
                enzyme.HexCode = GenerateLastHexCode();
                continue;
            }

            if (!_prototype.TryIndex<StructuralEnzymesPrototype>(enzyme.EnzymesPrototypeId, out var enzymePrototype))
                continue;

            if (enzymePrototype.TypeDeviation == EnzymesType.Disease)
            {
                enzyme.HexCode = GetHexCodeDisease();
            }
        }

        TryChangeStructuralEnzymes(component);

        Dirty(uid, component);
    }
    #endregion
}

public sealed class CureDnaDiseaseAttemptEvent : EntityEventArgs
{
    public float CureChance { get; }

    public CureDnaDiseaseAttemptEvent(float cureChance)
    {
        CureChance = cureChance;
    }
}

public sealed class MutateDnaAttemptEvent : EntityEventArgs
{
    public MutateDnaAttemptEvent()
    {
    }
}
