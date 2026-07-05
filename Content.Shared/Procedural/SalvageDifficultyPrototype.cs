// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System; // Dumont: Necessário para o TimeSpan funcionar
using Robust.Shared.Prototypes;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Procedural;

[Prototype]
public sealed partial class SalvageDifficultyPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = string.Empty;

    /// <summary>
    /// Color to be used in UI.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("color")]
    public Color Color = Color.White;

    /// <summary>
    /// How much loot this difficulty is allowed to spawn.
    /// </summary>
    [DataField("lootBudget", required : true)]
    public float LootBudget;

    /// <summary>
    /// How many mobs this difficulty is allowed to spawn.
    /// </summary>
    [DataField("mobBudget", required : true)]
    public float MobBudget;

    [DataField("lootPrototype")]
    public string LootPrototypeId = "SalvageLoot";

    /// <summary>
    /// Budget allowed for mission modifiers like no light, etc.
    /// </summary>
    [DataField("modifierBudget")]
    public float ModifierBudget;

    [DataField("recommendedPlayers", required: true)]
    public int RecommendedPlayers;

    // --- Dumont / Starlight Port ---

    /// <summary>
    /// Chance of this difficulty being picked in the random pool.
    /// </summary>
    [DataField("probability")]
    public float Probability = 1f;

    /// <summary>
    /// How much time needs to pass in the round before this difficulty can appear.
    /// </summary>
    [DataField("delay")]
    public TimeSpan Delay = TimeSpan.Zero;
}