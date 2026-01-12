// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 KillanGenifer <157119956+KillanGenifer@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._CorvaxGoob.Events.StatusEffects;

[Serializable, DataDefinition]
public sealed partial class ApplyStatusEffectTargetEvent : BaseTargetEvent
{
    [DataField]
    [AlwaysPushInheritance]
    public string Key = "";

    [DataField]
    [AlwaysPushInheritance]
    public float Time = 0;

    [DataField]
    [AlwaysPushInheritance]
    public bool Refresh = true;

    [DataField]
    [AlwaysPushInheritance]
    public string ComponentType;
}
