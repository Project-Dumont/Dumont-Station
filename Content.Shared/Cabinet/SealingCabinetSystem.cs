// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-License-Identifier: MIT

using Content.Shared.Emag.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;

namespace Content.Shared.Cabinet;

public sealed partial class SealingCabinetSystem : EntitySystem
{
    [Dependency] private ItemCabinetSystem _cabinet = default!;
    [Dependency] private OpenableSystem _openable = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SealingCabinetComponent, OpenableOpenAttemptEvent>(OnOpenAttempt);
        SubscribeLocalEvent<SealingCabinetComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnOpenAttempt(Entity<SealingCabinetComponent> ent, ref OpenableOpenAttemptEvent args)
    {
        if (!TryComp<ItemCabinetComponent>(ent, out var cabinet) || !_cabinet.HasItem((ent, cabinet)))
            return;

        args.Cancelled = true;
        if (args.User is {} user)
            _popup.PopupClient(Loc.GetString(ent.Comp.SealedPopup, ("container", ent.Owner)), ent, user);
    }

    private void OnEmagged(Entity<SealingCabinetComponent> ent, ref GotEmaggedEvent args)
    {
        if (!ent.Comp.Emaggable)
            return;

        if (!TryComp<ItemCabinetComponent>(ent, out var cabinet) || !_cabinet.HasItem((ent, cabinet)) || _openable.IsOpen(ent))
            return;

        _openable.SetOpen(ent, true);

        args.Handled = true;
        args.Repeatable = true;
    }
}
