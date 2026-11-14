// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Common.Traitor;

[ByRefEvent]
public record struct UplinkAssignedEvent(EntityUid Mind, EntityUid Host);
