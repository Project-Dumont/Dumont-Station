using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared.CompactPod;

public abstract partial class SharedCompactPodSystem : EntitySystem { }

/// <summary>
/// Event raised when a person enters a pod, on both success and failure
/// </summary>
[Serializable, NetSerializable]
public sealed partial class PodPassengerEntryEvent : SimpleDoAfterEvent
{
}
