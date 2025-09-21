// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Matthew Herber <32679887+Happyrobot33@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.Combat.Effects.EntitySystems;
using Robust.Shared.Map;

namespace Content.Client._Starlight.Combat.Effects.EntitySystems;

/// <summary>
/// Client-side implementation of the armor spark effect system.
/// </summary>
public sealed class ArmorSparkEffectSystem : SharedArmorSparkEffectSystem
{
    protected override void SpawnSparkEffectAt(EntityCoordinates coordinates, string effectPrototype)
    {
        // Client doesn't spawn effects directly - handled by server
    }

    protected override void PlayRicochetSound(EntityCoordinates coordinates, string soundCollection)
    {
        // Client doesn't play sounds directly - handled by server
    }
}
