using System;

namespace Content.Client._Dumont.Announce.Effects;

public interface IAnnouncementVisualEffect
{
    void Apply(AnnouncementEffectContext context, TimeSpan currentTime);
}
