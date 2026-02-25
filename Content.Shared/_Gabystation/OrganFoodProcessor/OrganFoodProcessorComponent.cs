// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT

using Content.Shared.Body.Systems;
using Robust.Shared.Audio;

namespace Content.Shared._Gabystation.OrganFoodProcessor;

[RegisterComponent, Access(typeof(StomachSystem), typeof(OrganFoodProcessorSystem))]
public sealed partial class OrganFoodProcessorComponent : Component
{
    [DataField]
    public SoundSpecifier ProcessingSound = new SoundCollectionSpecifier("OrganFoodProcessor")
    {
        Params = AudioParams.Default.WithVolume(0.5f),
    };

    [DataField]
    public TimeSpan JitterDuration = TimeSpan.FromSeconds(2.5f);

    [DataField]
    public float JitterFrequency = 300f;
}
