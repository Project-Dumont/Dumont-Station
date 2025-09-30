// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Matthew Herber <32679887+Happyrobot33@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Starlight.Combat.Effects.Components;

/// <summary>
/// Component for cyborgs that triggers spark effects when hit by any hitscan bullets.
/// Unlike ArmorSparkEffectComponent, this triggers on any bullet hit regardless of type or armor values.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CyborgSparkEffectComponent : Component
{
    /// <summary>
    /// The prototype ID of the spark effect entity to spawn.
    /// </summary>
    [DataField("sparkEffectPrototype")]
    public string SparkEffectPrototype = "ArmorSparkEffect";

    /// <summary>
    /// The sound collection to play when sparks are triggered.
    /// </summary>
    [DataField("ricochetSoundCollection")]
    public string RicochetSoundCollection = "armor_ricochet_cyborg";

    /// <summary>
    /// Maximum random offset in X and Y directions for spark positioning.
    /// </summary>
    [DataField("maxOffset")]
    public float MaxOffset = 0.3f;
}
