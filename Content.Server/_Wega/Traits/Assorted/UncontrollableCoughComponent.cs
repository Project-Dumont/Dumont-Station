// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Traits.Assorted;

[RegisterComponent]
public sealed partial class UncontrollableCoughComponent : Component
{
    [DataField("emote", customTypeSerializer: typeof(PrototypeIdSerializer<EmotePrototype>))]
    public string EmoteId = "Cough";

    [DataField("timeBetweenIncidents")]
    public Vector2 TimeBetweenIncidents = new(30, 120);

    public float NextIncidentTime;
}
