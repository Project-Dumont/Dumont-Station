// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenomorph;
using Content.Shared._White.Xenomorphs;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Goobstation.Shared._Trauma.Xenomorph;

public sealed class NeurotoxinGlandSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NeurotoxinGlandComponent, ToggleAcidSpitEvent>(OnToggle);
        SubscribeLocalEvent<BodyComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnToggle(Entity<NeurotoxinGlandComponent> ent, ref ToggleAcidSpitEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Active = !ent.Comp.Active;
        Dirty(ent);
        _popup.PopupClient(Loc.GetString(ent.Comp.Active ? "neurotoxin-gland-activated" : "neurotoxin-gland-deactivated"), args.Performer, args.Performer);
        args.Handled = true;
    }

    private void OnShotAttempted(Entity<BodyComponent> ent, ref ShotAttemptedEvent args)
    {
        if (args.Used.Owner != ent.Owner)
            return;

        foreach (var organ in _body.GetBodyOrgans(ent.Owner, ent.Comp))
        {
            if (TryComp<NeurotoxinGlandComponent>(organ.Id, out var gland) && !gland.Active)
            {
                args.Cancel();
                return;
            }
        }
    }
}
