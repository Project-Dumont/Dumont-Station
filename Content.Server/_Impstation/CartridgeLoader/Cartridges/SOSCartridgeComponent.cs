// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._Impstation.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class SOSCartridgeComponent : Component
{
    [DataField]
    /// <summary>
    /// Path to PDA ID
    /// </summary>
    public const string PDAIdContainer = "PDA-id";

    /// <summary>
    /// Name to use in case there is none, localized
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedDefaultName => Loc.GetString("sos-caller-defaultname");

    [DataField]
    /// <summary>
    /// Notification message
    /// </summary>
    public string HelpMessage = "sos-message";

    /// <summary>
    /// Message used to call help
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public string LocalizedHelpMessage => Loc.GetString(HelpMessage);

    [DataField]
    /// <summary>
    /// Channel to notify
    /// </summary>
    public ProtoId<RadioChannelPrototype> HelpChannel = "Security";

    [DataField]
    /// <summary>
    /// Timeout between calls
    /// </summary>
    public const float TimeOut = 90;

    [DataField]
    /// <summary>
    /// Time at which a next SOS call is now allowed
    /// </summary>
    public TimeSpan NextMinimumTime = TimeSpan.FromSeconds(0);
}
