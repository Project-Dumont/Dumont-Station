// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Gabystation.Interaction.Events;
using Content.Shared._Gabystation.Interaction.Components;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Shared._Gabystation.Interaction.EntitySystems;

public sealed class BindRecallSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BoundRecallComponent, GetVerbsEvent<ActivationVerb>>(OnGetVerb);
        SubscribeLocalEvent<BoundRecallComponent, BindRecallDoAfterEvent>(OnBindDoAfter);
        SubscribeLocalEvent<BoundRecallComponent, UnbindRecallDoAfterEvent>(OnUnbindDoAfter);
        SubscribeLocalEvent<BoundRecallComponent, ExaminedEvent>(OnExamine);
    }

    private void OnGetVerb(Entity<BoundRecallComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanInteract)
            return;

        var user = args.User;
        var recallComp = CompOrNull<RecallBoundItemComponent>(user);

        // ------------------
        // Bind verb
        // ------------------
        if (ent.Comp.BoundUser == null)
        {
            var bindVerb = new ActivationVerb
            {
                Text = Loc.GetString("recall-item-bind"),
                Act = () =>
                {
                    var recall = EnsureComp<RecallBoundItemComponent>(user);

                    // Check BEFORE the DoAfter
                    if (recall.BoundItem != null)
                    {
                        _popup.PopupEntity(Loc.GetString("recall-item-already-have"), user, user);
                        return;
                    }

                    var doAfter = new DoAfterArgs(EntityManager, user, 5f,
                        new BindRecallDoAfterEvent(),
                        ent.Owner)
                    {
                        BreakOnMove = true,
                        BreakOnDamage = true,
                        NeedHand = true
                    };

                    _doAfter.TryStartDoAfter(doAfter);
                }
            };

            args.Verbs.Add(bindVerb);
        }

        // ------------------
        // Unbind verb
        // ------------------
        if (ent.Comp.BoundUser == user && recallComp != null)
        {
            var unbindVerb = new ActivationVerb
            {
                Text = Loc.GetString("recall-item-unbind"),
                Act = () =>
                {
                    var doAfter = new DoAfterArgs(EntityManager, user, 5f,
                        new UnbindRecallDoAfterEvent(),
                        ent.Owner)
                    {
                        BreakOnMove = true,
                        BreakOnDamage = true,
                        NeedHand = true
                    };

                    _doAfter.TryStartDoAfter(doAfter);
                }
            };

            args.Verbs.Add(unbindVerb);
        }
    }

    private void OnBindDoAfter(Entity<BoundRecallComponent> ent, ref BindRecallDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        var user = args.User;

        var recall = EnsureComp<RecallBoundItemComponent>(user);

        if (recall.BoundItem != null)
            return;

        // Verify if another user has already bound
        if (ent.Comp.BoundUser != null)
        {
            _popup.PopupEntity(Loc.GetString("recall-item-already-bound"), user, user);
            return;
        }

        EntityUid? action = null;
        _actions.AddAction(user, ref action, recall.RecallAction);

        if (action == null)
            return;

        recall.BoundItem = ent.Owner;
        recall.RecallActionEntity = action;

        ent.Comp.BoundUser = user;
        Dirty(ent);

        var itemName = Name(ent.Owner);

        _metaData.SetEntityName(action.Value,
            Loc.GetString("recall-item-action-name", ("item", itemName)));

        _metaData.SetEntityDescription(action.Value,
            Loc.GetString("recall-item-action-desc", ("item", itemName)));
    }

    private void OnUnbindDoAfter(Entity<BoundRecallComponent> ent, ref UnbindRecallDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        var user = args.User;
        var recallComp = CompOrNull<RecallBoundItemComponent>(user);

        if (recallComp == null)
            return;

        // Verify if the unbinding item is the one bound to the user
        if (recallComp.BoundItem != ent.Owner)
            return;

        if (recallComp.RecallActionEntity != null &&
            Exists(recallComp.RecallActionEntity.Value))
        {
            QueueDel(recallComp.RecallActionEntity.Value);
        }

        recallComp.BoundItem = null;
        recallComp.RecallActionEntity = null;
        Dirty(user, recallComp);

        ent.Comp.BoundUser = null;
        Dirty(ent);
    }

    private void OnExamine(Entity<BoundRecallComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.Examinable)
            return;

        if (!args.IsInDetailsRange)
            return;

        if (ent.Comp.BoundUser == null)
        {
            args.PushMarkup(Loc.GetString("recall-bound-item-examine-free"));
        }
        else
        {
            args.PushMarkup(Loc.GetString("recall-bound-item-examine-owned"));
        }
    }
}
