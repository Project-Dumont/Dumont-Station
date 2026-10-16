// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.EntityEffects;
using Content.Trauma.Shared.Heretic.Events;
using Robust.Shared.Audio;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticBladeComponent : Component
{
    [DataField]
    public HereticPath? Path;

    [DataField]
    public EntityEffect[]? ThrowEffects;

    [DataField]
    public EntityEffect[]? Effects;

    [DataField, NonSerialized]
    public HereticBladeBonusEvent? BonusEvent;

    /// <summary>
    /// Path stage -> effect probability
    /// </summary>
    [DataField]
    public Dictionary<int, float> Probabilities = new()
    {
        { 0, 1f },
    };

    [DataField]
    public SoundSpecifier? ShatterSound = new SoundCollectionSpecifier("GlassBreak");

    [DataField]
    public SoundSpecifier ArrivalSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    [DataField]
    public SoundSpecifier DepartureSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
