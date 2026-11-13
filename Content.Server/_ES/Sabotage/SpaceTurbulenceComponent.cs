// SPDX-FileCopyrightText: 2026 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2017-2026 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._ES.Sabotage;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class SpaceTurbulenceComponent : Component
{
    [DataField]
    public MinMax JerkRange = new(3, 5);

    [DataField]
    public TimeSpan MinWarningTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan MaxWarningTime = TimeSpan.FromSeconds(15);

    [DataField]
    public TimeSpan MinJerkDelay = TimeSpan.FromSeconds(2.5);

    [DataField]
    public TimeSpan MaxJerkDelay = TimeSpan.FromSeconds(7);

    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(3);

    [DataField]
    public float ThrowDistance = 15;

    [DataField]
    public float ThrowForce = 10;

    [DataField]
    public int RemainingJerks;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextJerk;
}
