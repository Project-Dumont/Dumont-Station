// SPDX-License-Identifier: AGPL-3.0-or-later

using System;

namespace Content.Client._Dumont.Announce.Effects;

public interface IAnnouncementVisualEffect
{
    void Apply(AnnouncementEffectContext context, TimeSpan currentTime);
}
