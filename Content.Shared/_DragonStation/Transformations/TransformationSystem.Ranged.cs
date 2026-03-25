using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._DragonStation.Transformations;

public sealed partial class TransformationSystem
{
    private void InitializeRanged()
    {
        SubscribeLocalEvent<TransformationComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    // Melee-focused transformations can disable guns while active.
    private void OnShotAttempted(EntityUid uid, TransformationComponent component, ref ShotAttemptedEvent args)
    {
        if (!component.Active || !component.BlockRangedWeapons)
            return;

        args.Cancel();

        if (!_net.IsServer)
            return;

        if (_timing.CurTime < component.NextRangedPopup)
            return;

        component.NextRangedPopup = _timing.CurTime + component.RangedPopupCooldown;
        _popup.PopupEntity(Loc.GetString(component.BlockedRangedPopup), uid, uid);
    }

    // Force-drop any held guns on transformation so the user commits to close combat.
    private void DropHeldGuns(EntityUid uid, TransformationComponent component)
    {
        if (!component.BlockRangedWeapons)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        foreach (var (handName, _) in hands.Hands)
        {
            if (!_hands.TryGetHeldItem((uid, hands), handName, out var held))
                continue;

            if (!HasComp<GunComponent>(held.Value))
                continue;

            _hands.TryDrop((uid, hands), handName, checkActionBlocker: false);
        }
    }
}
