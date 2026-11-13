// SPDX-FileCopyrightText: 2026 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2017-2026 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._ES.Sabotage;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class TraitorBuggableComponent : Component
{
    [DataField]
    public TimeSpan PlantTime = TimeSpan.FromSeconds(3);

    [DataField]
    public TimeSpan RemoveTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan Duration = TimeSpan.FromMinutes(5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan StartedAt;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan EndsAt;

    [DataField]
    public HashSet<EntityUid> Objectives = new();
}
