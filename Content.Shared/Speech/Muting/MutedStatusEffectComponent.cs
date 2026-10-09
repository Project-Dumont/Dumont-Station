// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
// Dumont end

using Robust.Shared.GameStates;

namespace Content.Shared.Speech.Muting;

/// <summary>
/// Marks a status effect that prevents speaking, screaming, and vocal emotes.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MutedStatusEffectComponent : Component
{
    /// <summary>
    /// Popup shown when speech is blocked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public LocId SpeakPopup = "speech-muted";

    /// <summary>
    /// Popup shown when screaming is blocked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public LocId ActionPopup = "speech-muted";
}
