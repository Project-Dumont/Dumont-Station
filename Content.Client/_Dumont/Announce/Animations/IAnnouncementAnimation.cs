// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Client._Dumont.Announce.Animations;

public interface IAnnouncementAnimation
{
    void Reset(AnnouncementAnimationContext context);
    AnnouncementAnimationStatus Update(AnnouncementAnimationContext context, float deltaTime);
}
