using Content.Shared.Actions;
using Robust.Shared.Audio;

namespace Content.Shared.Genetics;

public sealed partial class HulkChargeActionEvent : EntityWorldTargetActionEvent
{
    public SoundSpecifier Sound = new SoundCollectionSpecifier("FootstepThud");
}
