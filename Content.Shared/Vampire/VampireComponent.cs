// SPDX-FileCopyrightText: 2024 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dreykor <160512778+Dreykor@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dreykor <Dreykor12@gmail.com>
// SPDX-FileCopyrightText: 2025 Dreykor <arguemeu@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Prototypes;
using Content.Shared.StatusIcon;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.StatusEffect;
using Content.Shared.Store;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Vampire.Components;

[RegisterComponent]
public sealed partial class VampireComponent : Component
{
    //Static prototype references
    public static readonly EntProtoId SleepStatusEffectProto = "StatusEffectForcedSleeping";
    [ValidatePrototypeId<EmotePrototype>]
    public static readonly string ScreamEmoteProto = "Scream";
    [ValidatePrototypeId<CurrencyPrototype>]
    public static readonly string CurrencyProto = "BloodEssence";

    [ViewVariables(VVAccess.ReadOnly), DataField("defaultMutation")]
    public VampireMutationsType DefaultMutation = VampireMutationsType.None;
    [ViewVariables(VVAccess.ReadOnly), DataField("currentMutation")]
    public VampireMutationsType CurrentMutation = VampireMutationsType.None;

    public readonly HashSet<VampireMutationsType> VampireMutations = new()
    {
        VampireMutationsType.None,
        VampireMutationsType.Hemomancer,
        VampireMutationsType.Umbrae,
        VampireMutationsType.Gargantua,
        //VampireMutationsType.Dantalion,
        VampireMutationsType.Bestia
    };

    public static readonly EntityWhitelist AcceptableFoods = new()
    {
        Tags = new() { "Pill" }
    };
    [ValidatePrototypeId<MetabolizerTypePrototype>]
    public static readonly string MetabolizerVampire = "Vampire";
    [ValidatePrototypeId<MetabolizerTypePrototype>]
    public static readonly string MetabolizerBloodsucker = "Bloodsucker";

    public static readonly DamageSpecifier MeleeDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>() { { "Slash", 10 } }
    };
    public static readonly DamageSpecifier HolyDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>() { { "Burn", 10 } }
    };

    [ValidatePrototypeId<EntityPrototype>]
    public static readonly string MutationsActionPrototype = "ActionVampireOpenMutationsMenu";

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? MutationsAction;

    public readonly List<ProtoId<EntityPrototype>> BaseVampireActions = new()
    {
        "ActionVampireToggleFangs",
        "ActionVampireHypnotise"
    };

    [ValidatePrototypeId<VampirePowerPrototype>]
    public static readonly string DrinkBloodPrototype = "DrinkBlood";

    /// <summary>
    /// Total blood drank, counter for end of round screen
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float TotalBloodDrank = 0;

    /// <summary>
    /// How much blood per mouthful
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float MouthVolume = 5;

    /// <summary>
    /// All unlocked abilities
    /// </summary>
    public Dictionary<string, EntityUid?> UnlockedPowers = new();

    /// <summary>
    /// Current available balance, used to sync currency across heirlooms and add essence as we feed
    /// </summary>
    public Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> Balance = default!;

    public readonly SoundSpecifier BloodDrainSound = new SoundPathSpecifier("/Audio/Items/drink.ogg", new AudioParams() { Volume = -3f, MaxDistance = 3f });
    public readonly SoundSpecifier AbilityPurchaseSound = new SoundPathSpecifier("/Audio/Items/drink.ogg");
}


/// <summary>
/// Contains all details about the ability and its effects or restrictions
/// </summary>
[DataDefinition]
[Prototype("vampirePower")]
public sealed partial class VampirePowerPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; }

    [DataField]
    public float ActivationCost = 0;
    [DataField]
    public bool UsableWhileCuffed = true;
    [DataField]
    public bool UsableWhileStunned = true;
    [DataField]
    public bool UsableWhileMuffled = true;
    [DataField]
    public DamageSpecifier? Damage = default!;
    [DataField]
    public TimeSpan? Duration = TimeSpan.Zero;
    [DataField]
    public TimeSpan? DoAfterDelay = TimeSpan.Zero;
    [DataField]
    public string? PolymorphTarget = default!;
    [DataField]
    public float Upkeep = 0;
    // Cooldown for this power
    [DataField]
    public TimeSpan Cooldown = TimeSpan.Zero;
    // Collective Mind (Hivemind) para vampiros
    [DataField]
    public ProtoId<CollectiveMindPrototype> CollectiveMindAdded = "VampireMind";
}

[DataDefinition]
[Prototype("vampirePassive")]
public sealed partial class VampirePassivePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; }

    [DataField(required: true)]
    public string CatalogEntry = string.Empty;

    [DataField]
    public ComponentRegistry CompsToAdd = new();

    [DataField]
    public ComponentRegistry CompsToRemove = new();
}

/// <summary>
/// Marks an entity as taking damage when hit by a bible, rather than being healed
/// </summary>
[RegisterComponent]
public sealed partial class UnholyComponent : Component { }

/// <summary>
/// Marks a container as a coffin, for the purposes of vampire healing
/// </summary>
[RegisterComponent]
public sealed partial class CoffinComponent : Component { }

[RegisterComponent]
public sealed partial class VampireFangsExtendedComponent : Component { }

/// <summary>
/// When added, damage the entity if its on the space
/// </summary>
[RegisterComponent]
public sealed partial class VampireSpaceDamageComponent : Component
{
    public double NextSpaceDamageTick = 0;
}

/// <summary>
/// When added, heals the entity by the specified amount
/// </summary>
[RegisterComponent]
public sealed partial class VampireHealingComponent : Component
{
    public double NextHealTick = 0;

    public DamageSpecifier? Healing = default!;
}

[RegisterComponent]
public sealed partial class VampireDeathsEmbraceComponent : Component
{
    [ViewVariables()]
    public EntityUid? HomeCoffin = default!;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float Cost = 0;

    [DataField]
    public DamageSpecifier CoffinHealing = default!;
}
[RegisterComponent]
public sealed partial class VampireStealthComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float NextStealthTick = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Upkeep = 0;
}

[Serializable, NetSerializable]
public enum VampireMutationsType : byte
{
    None,
    Hemomancer,
    Umbrae,
    Gargantua,
    Dantalion,
    Bestia
}

[Serializable, NetSerializable]
public sealed class VampireMutationComponentState : ComponentState
{
    public VampireMutationsType SelectedMutation;
}

[Serializable, NetSerializable]
public sealed class VampireMutationBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly HashSet<VampireMutationsType> MutationList;
    public readonly VampireMutationsType SelectedMutation;

    public VampireMutationBoundUserInterfaceState(HashSet<VampireMutationsType> mutationList, VampireMutationsType selectedId)
    {
        MutationList = mutationList;
        SelectedMutation = selectedId;
    }
}

[Serializable, NetSerializable]
public sealed class VampireMutationPrototypeSelectedMessage : BoundUserInterfaceMessage
{
    public readonly VampireMutationsType SelectedId;

    public VampireMutationPrototypeSelectedMessage(VampireMutationsType selectedId)
    {
        SelectedId = selectedId;
    }
}

[Serializable, NetSerializable]
public enum VampireMutationUiKey : byte
{
    Key
}

[NetSerializable, Serializable]
public enum VampireVisualLayers : byte
{
    Digit1,
    Digit2,
    Digit3
}

/*[Serializable, NetSerializable]
public enum VampirePowerKey : byte
{
    ToggleFangs,
    Glare,
    DeathsEmbrace,
    Screech,
    Hypnotise,
    Polymorph,
    NecroticTouch,
    BloodSteal,
    CloakOfDarkness,
    StellarWeakness,
    SummonHeirloom,

    //Passives
    UnnaturalStrength,
    SupernaturalStrength
}*/
