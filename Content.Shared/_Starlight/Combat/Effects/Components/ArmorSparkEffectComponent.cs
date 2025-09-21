// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Matthew Herber <32679887+Happyrobot33@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Starlight.Combat.Effects.Components;

/// <summary>
/// Component that marks armor pieces as capable of producing spark effects
/// when hit by SP or HP hitscan bullets with sufficient pierce resistance or Rock material.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ArmorSparkEffectComponent : Component
{
    /// <summary>
    /// The prototype ID of the spark effect entity to spawn.
    /// </summary>
    [DataField("sparkEffectPrototype")]
    public string SparkEffectPrototype = "ArmorSparkEffect";

    /// <summary>
    /// The sound collection ID to play when sparks occur.
    /// </summary>
    [DataField("ricochetSoundCollection")]
    public string RicochetSoundCollection = "armor_ricochet";

    /// <summary>
    /// Maximum random offset for spark positioning within the tile.
    /// </summary>
    [DataField("maxOffset")]
    public float MaxOffset = 0.4f;
}
