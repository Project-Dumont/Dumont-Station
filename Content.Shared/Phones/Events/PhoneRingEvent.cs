// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Phones.Components;

namespace Content.Shared.Phones.Events;

[ByRefEvent]
public record struct PhoneRingEvent(Entity<RotaryPhoneComponent> Phone);
