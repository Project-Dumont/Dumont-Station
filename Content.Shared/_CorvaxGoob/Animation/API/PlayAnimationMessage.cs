// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 KillanGenifer <157119956+KillanGenifer@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.Animation.API;

[Serializable, NetSerializable]
public sealed class PlayAnimationMessage : EntityEventArgs
{
    public PlayAnimationMessage(NetEntity animatedEntity, string animationID)
    {
        this.AnimatedEntity = animatedEntity;
        this.AnimationID = animationID;
    }

    public NetEntity AnimatedEntity;

    public string AnimationID = "";
}
