// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.PDA;

/// <summary>
/// An event that notifies all pda in a given notification group (see prototype).
/// Can take optional isLoud and station arguments, for making the pda ring when notified and filtering for a given station respectively
/// </summary>
public sealed partial class PdaNotificationEvent(string message, ProtoId<NotificationGroupPrototype> group, bool isLoud = false, EntityUid? station = null) : HandledEntityEventArgs {
    /// <summary>
    /// The notification message
    /// </summary>
    public readonly string Message = message;

    /// <summary>
    /// The id of the notification group
    /// </summary>
    public readonly ProtoId<NotificationGroupPrototype> Group = group;

    /// <summary>
    /// Determines whether the notified pdas will ring when receiving a message
    /// </summary>
    public readonly bool IsLoud = isLoud;

    /// <summary>
    /// Filters for station
    /// </summary>
    public readonly EntityUid? Station = station;
}
