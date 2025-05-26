using Robust.Shared.Serialization;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Shared.Weapons.Ranged.Events;

public sealed class TryFireJammedWeapon : EntityEventArgs
{
    public EntityUid GunUid;
    public EntityUid? GunUser;
    public GunComponent GunComponent;


    public TryFireJammedWeapon(EntityUid gunUid, EntityUid? gunUser, GunComponent gunComponent)
    {
        GunUid = gunUid;
        GunUser = gunUser;
        GunComponent = gunComponent;
    }
}
