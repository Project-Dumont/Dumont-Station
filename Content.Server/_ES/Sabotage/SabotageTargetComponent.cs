// SPDX-FileCopyrightText: 2025-2026 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2017-2026 Space Wizards Federation
// SPDX-License-Identifier: MIT

namespace Content.Server._ES.Sabotage;

[RegisterComponent]
public sealed partial class SabotageTargetComponent : Component
{
    [DataField]
    public TimeSpan SabotageTime = TimeSpan.FromSeconds(10);

    [DataField]
    public TimeSpan RepairTime = TimeSpan.FromSeconds(15);

    [DataField]
    public bool Sabotaged;

    [DataField]
    public int CorruptedAnnouncements;
}

[ByRefEvent]
public record struct SabotageEffectEvent(EntityUid User, bool Handled = false);
