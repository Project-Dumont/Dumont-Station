using Content.Shared._Dumont.Announce;
using Robust.Shared.Random;

namespace Content.Client._Dumont.Announce.Animations;

public static class AnnouncementAnimationFactory
{
    public static IAnnouncementAnimation Create(AnnouncementStyle style, IRobustRandom random)
    {
        return style.AnimationConfig.Animation switch
        {
            AnnouncementAnimation.None => new NoneAnimation(),
            AnnouncementAnimation.Typewriter => new TypewriterAnimation(),
            AnnouncementAnimation.Glitch => new GlitchAnimation(),
            AnnouncementAnimation.Slide => new SlideAnimation(),
            AnnouncementAnimation.Zoom => new ZoomAnimation(),
            AnnouncementAnimation.Bounce => new BounceAnimation(),
            AnnouncementAnimation.Fade => new FadeAnimation(),
            AnnouncementAnimation.Pulse => new PulseAnimation(),
            AnnouncementAnimation.Heartbeat => new HeartbeatAnimation(),
            AnnouncementAnimation.Warp => new WarpAnimation(),
            _ => new NoneAnimation()
        };
    }
}
