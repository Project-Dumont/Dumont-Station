// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 KillanGenifer <157119956+KillanGenifer@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._CorvaxGoob.Animation.API;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.Animation;

[Prototype("animation")]
[Serializable, NetSerializable, DataDefinition]
public sealed partial class AnimationPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public List<AnimationTrackData> Tracks = new();

    /// <summary>
    ///     Total animation length in seconds. Will fully stop animation on ends.
    /// </summary>
    [DataField(required: true)]
    public float Length = 0;
}
