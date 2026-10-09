using Content.Server.BombDefusal.Components;
using Content.Server.Popups;
using Content.Shared.BombDefusal;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server.BombDefusal.Systems;

/// <summary>
/// Handles the Bomb Defusal Kit item logic.
/// The kit must be wielded to interact with bombs. Dropping or unwielding the kit
/// while the bomb UI is open closes the UI (triggering the existing strike-on-close penalty).
/// </summary>
public sealed class BombDefusalKitSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        // When a player uses the kit on a bomb (interact-using)
        SubscribeLocalEvent<BombDefusalKitComponent, AfterInteractEvent>(OnAfterInteract);

        // When the kit is unwielded, close the bomb UI
        SubscribeLocalEvent<BombDefusalKitComponent, ItemUnwieldedEvent>(OnKitUnwielded);

        // When the kit leaves the hand entirely
        SubscribeLocalEvent<BombDefusalKitComponent, GotUnequippedHandEvent>(OnKitDropped);
    }

    private void OnAfterInteract(EntityUid uid, BombDefusalKitComponent comp, AfterInteractEvent args)
    {
        if (args.Handled || args.Target == null || !args.CanReach)
            return;

        var target = args.Target.Value;

        // Only works on entities with BombDefusalComponent
        if (!TryComp<BombDefusalComponent>(target, out _))
            return;

        // Kit must be wielded
        if (!TryComp<WieldableComponent>(uid, out var wieldable) || !wieldable.Wielded)
        {
            _popup.PopupEntity(Loc.GetString("bomb-defusal-kit-must-wield"), uid, args.User, PopupType.MediumCaution);
            args.Handled = true;
            return;
        }

        // Check if the bomb has a UI we can open
        if (!_ui.HasUi(target, BombDefusalUiKey.Key))
            return;

        // Open the bomb's UI for this player
        if (_ui.IsUiOpen(target, BombDefusalUiKey.Key, args.User))
        {
            _ui.CloseUi(target, BombDefusalUiKey.Key, args.User);
            comp.LinkedBomb = null;
        }
        else
        {
            _ui.OpenUi(target, BombDefusalUiKey.Key, args.User);
            comp.LinkedBomb = target;
        }

        Dirty(uid, comp);
        args.Handled = true;
    }

    private void OnKitUnwielded(EntityUid uid, BombDefusalKitComponent comp, ItemUnwieldedEvent args)
    {
        CloseLinkedBombUi(uid, comp, args.User);
    }

    private void OnKitDropped(EntityUid uid, BombDefusalKitComponent comp, GotUnequippedHandEvent args)
    {
        CloseLinkedBombUi(uid, comp, args.User);
    }

    private void CloseLinkedBombUi(EntityUid kitUid, BombDefusalKitComponent comp, EntityUid user)
    {
        if (comp.LinkedBomb == null)
            return;

        var bomb = comp.LinkedBomb.Value;

        if (!Exists(bomb))
        {
            comp.LinkedBomb = null;
            Dirty(kitUid, comp);
            return;
        }

        // Close the bomb UI for this user — this will trigger OnUiClosed in BombDefusalSystem
        // which handles the strike penalty
        if (_ui.HasUi(bomb, BombDefusalUiKey.Key))
        {
            _ui.CloseUi(bomb, BombDefusalUiKey.Key, user);
        }

        comp.LinkedBomb = null;
        Dirty(kitUid, comp);
    }
}
