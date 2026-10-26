using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Actions;
namespace Content.Shared._CMU14.Xenomorphs;
public sealed partial class XenomorphSpitActionEvent : WorldTargetActionEvent
{
    [DataField] public bool Scattered;
    [DataField] public bool Acid;
    [DataField] public bool Bone;
}
