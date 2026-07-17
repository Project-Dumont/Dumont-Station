// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.PDA;

public sealed partial class PdaNotificationEvent(string message, ProtoId<NotificationGroupPrototype> group, bool isLoud = false) : HandledEntityEventArgs {
    public readonly string Message = message;
    public readonly string Group = group;
    public readonly string IsLoud = isLoud;
}
