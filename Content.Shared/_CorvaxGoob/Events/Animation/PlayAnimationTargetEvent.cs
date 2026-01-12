// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 KillanGenifer <157119956+KillanGenifer@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._CorvaxGoob.Events.Animation;

[Serializable, DataDefinition]
public sealed partial class PlayAnimationTargetEvent : BaseTargetEvent
{
    [DataField]
    [AlwaysPushInheritance]
    public string AnimationID = "";
}
