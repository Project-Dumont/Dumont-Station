// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 KillanGenifer <157119956+KillanGenifer@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._CorvaxGoob.Events;

namespace Content.Server._CorvaxGoob.Events.Components;

[RegisterComponent]
public sealed partial class ExecuteTargetEventsOnTriggerComponent : Component
{
    [DataField]
    public List<BaseTargetEvent> Events;
}
