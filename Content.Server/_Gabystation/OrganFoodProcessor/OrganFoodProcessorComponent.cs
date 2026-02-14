// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT

using Content.Server.Body.Systems;
using Robust.Shared.Audio;


namespace Content.Server._Gabystation.OrganFoodProcessor
{
    [RegisterComponent, Access(typeof(StomachSystem), typeof(OrganFoodProcessorSystem))]
    public sealed partial class OrganFoodProcessorComponent : Component
    {
        [DataField("processingSound")]
        public SoundSpecifier ProcessingSound = new SoundCollectionSpecifier("OrganFoodProcessor");
    }
}
