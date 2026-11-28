// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Dumont.Triage;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
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
    [Dependency] private readonly TriageHudSystem _hud = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TriageTaggerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<TriageTaggerComponent, TriageTaggerSetMessage>(OnLevelSelected);

        // Janela do medhud que fica no paciente
        SubscribeLocalEvent<MobStateComponent, TriageTaggerSetMessage>(OnHudLevelSelected);
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

        ApplyTriage(args.Actor, target.Value, args.Level, component.Levels);
    }

    private void OnHudLevelSelected(EntityUid uid, MobStateComponent component, TriageTaggerSetMessage args)
    {
        // A mesma mensagem também é usada pelo aparelho então só vale a que veio da janela do medhud
        if (!args.UiKey.Equals(TriageHudUiKey.Key))
            return;

        // Tudo é conferido de novo aqui
        if (TerminatingOrDeleted(uid) || !_hud.TryGetHud(args.Actor, out var hud))
            return;

        if (!_interaction.InRangeUnobstructed(args.Actor, uid))
            return;

        _ui.CloseUi(uid, TriageHudUiKey.Key);

        ApplyTriage(args.Actor, uid, args.Level, hud.Levels);
    }

    /// <summary>
    /// Marca o paciente com a prioridade escolhida, ou tira a marcação se for nula
    /// A prioridade precisa estar na lista que o medhud mostra
    /// </summary>
    public void ApplyTriage(
        EntityUid user,
        EntityUid target,
        ProtoId<TriageIconPrototype>? picked,
        List<ProtoId<TriageIconPrototype>> allowed)
    {
        var name = Identity.Entity(target, EntityManager);

        if (picked is not { } level)
        {
            if (!HasComp<TriageTagComponent>(target))
                return;

            RemComp<TriageTagComponent>(target);
            _popup.PopupEntity(Loc.GetString("triage-tagger-removed", ("target", name)), target, user);
            return;
        }

        if (!allowed.Contains(level) || !_proto.TryIndex(level, out var proto))
            return;

        var tag = EnsureComp<TriageTagComponent>(target);
        tag.Level = level;
        Dirty(target, tag);

        _popup.PopupEntity(
            Loc.GetString("triage-tagger-applied", ("target", name), ("level", Loc.GetString(proto.Name))),
            target,
            user);
    }
}
