namespace Content.Client._Dumont.Announce.Animations;

public sealed class NoneAnimation : IAnnouncementAnimation
{
    public void Reset(AnnouncementAnimationContext context)
    {
        context.SetAllLabels();
    }

    public AnnouncementAnimationStatus Update(AnnouncementAnimationContext context, float deltaTime)
    {
        return AnnouncementAnimationStatus.Finished;
    }
}
