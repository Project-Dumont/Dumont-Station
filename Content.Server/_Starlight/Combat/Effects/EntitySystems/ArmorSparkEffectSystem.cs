// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Matthew Herber <32679887+Happyrobot33@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.Combat.Effects.EntitySystems;
using Content.Shared._Starlight.Combat.Effects.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Server._Starlight.Combat.Effects.EntitySystems;

/// <summary>
/// Server-side implementation of the armor spark effect system.
/// </summary>
public sealed class ArmorSparkEffectSystem : SharedArmorSparkEffectSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    protected override void SpawnSparkEffectAt(EntityCoordinates coordinates, string effectPrototype)
    {
        // Spawn the spark effect entity at the specified coordinates
        Spawn(effectPrototype, coordinates);
    }

    protected override void PlayRicochetSound(EntityCoordinates coordinates, string soundCollection)
    {
        // Play the ricochet sound at the specified coordinates
        var soundSpec = new SoundCollectionSpecifier(soundCollection);
        _audio.PlayPvs(soundSpec, coordinates);
    }
}
