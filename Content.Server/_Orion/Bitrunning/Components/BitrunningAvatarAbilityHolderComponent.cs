namespace Content.Server._Orion.Bitrunning.Components;

/// <summary>
/// Tracks actions granted to an avatar by bitrunning disks currently in its inventory tree.
/// </summary>
[RegisterComponent]
public sealed partial class BitrunningAvatarAbilityHolderComponent : Component
{
    public Dictionary<EntityUid, EntityUid?> ActionsByDisk = new();
}
