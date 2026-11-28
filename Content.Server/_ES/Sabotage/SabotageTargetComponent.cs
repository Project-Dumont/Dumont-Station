// SPDX-FileCopyrightText: 2025-2026 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2017-2026 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Content.Shared.Access;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._ES.Sabotage;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class SabotageTargetComponent : Component
{
    [DataField]
    public TimeSpan SabotageTime = TimeSpan.FromSeconds(12);

    [DataField]
    public TimeSpan RepairTime = TimeSpan.FromSeconds(15);

    [DataField]
    public bool Sabotaged;

    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>> RepairAccess = new()
    {
        "Engineering", "Command", "CentralCommand", "BlueshieldOfficer", "NanotrasenRepresentative",
    };

    [DataField]
    public HashSet<DoAfterId> ActiveDoAfters = new();

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextSpark;
}

[ByRefEvent]
public record struct SabotageEffectEvent(EntityUid User, bool Handled = false);
