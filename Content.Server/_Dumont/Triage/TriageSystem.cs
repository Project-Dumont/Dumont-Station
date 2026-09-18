// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Dumont.Triage;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.Triage;

public sealed class TriageSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriageTaggerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<TriageTaggerComponent, TriageTaggerSetMessage>(OnLevelSelected);
    }

    private void OnAfterInteract(Entity<TriageTaggerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { Valid: true } target)
            return;

        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, target))
            return;

        args.Handled = true;

        ent.Comp.Target = GetNetEntity(target);
        Dirty(ent);

        _ui.TryOpenUi(ent.Owner, TriageTaggerUiKey.Key, args.User);
    }

    private void OnLevelSelected(EntityUid uid, TriageTaggerComponent component, TriageTaggerSetMessage args)
    {
        if (component.Target is not { } netTarget || !TryGetEntity(netTarget, out var target))
            return;

        if (TerminatingOrDeleted(target.Value) || _whitelist.IsWhitelistFail(component.Whitelist, target.Value))
            return;

        if (!_interaction.InRangeUnobstructed(args.Actor, target.Value))
            return;

        _ui.CloseUi(uid, TriageTaggerUiKey.Key);

        var name = Identity.Entity(target.Value, EntityManager);

        if (args.Level is not { } level)
        {
            if (!HasComp<TriageTagComponent>(target.Value))
                return;

            RemComp<TriageTagComponent>(target.Value);
            _popup.PopupEntity(Loc.GetString("triage-tagger-removed", ("target", name)), target.Value, args.Actor);
            return;
        }

        if (!component.Levels.Contains(level) || !_proto.TryIndex(level, out var proto))
            return;

        var tag = EnsureComp<TriageTagComponent>(target.Value);
        tag.Level = level;
        Dirty(target.Value, tag);

        _popup.PopupEntity(
            Loc.GetString("triage-tagger-applied", ("target", name), ("level", Loc.GetString(proto.Name))),
            target.Value,
            args.Actor);
    }
}
