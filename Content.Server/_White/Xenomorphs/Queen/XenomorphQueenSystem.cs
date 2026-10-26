using Content.Server._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Server._White.Xenomorphs.Evolution;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Server.Actions;
using Content.Server.Popups;
using Content.Shared._White.Actions;
using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Xenomorphs.Queen;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Server._White.Xenomorphs.Queen;

public sealed partial class XenomorphQueenSystem : EntitySystem
{
    [Dependency] private ActionsSystem _actions = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private XenomorphEvolutionSystem _xenomorphEvolution = default!;

    [Dependency] private MobStateSystem _mobState = default!; // Dumont

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphQueenComponent, PromotionActionEvent>(OnPromotionAction);
        SubscribeLocalEvent<XenomorphQueenComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<XenomorphQueenComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnMapInit(EntityUid uid, XenomorphQueenComponent component, MapInitEvent args)
    {
        if (!HasComp<Content.Shared._Dumont.Xenomorphs.XenomorphRoyalComponent>(uid))
            _actions.AddAction(uid, ref component.PromotionAction, component.PromotionActionId);
    }

    private void OnShutdown(EntityUid uid, XenomorphQueenComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.PromotionAction);

    private void OnPromotionAction(EntityUid uid, XenomorphQueenComponent component, PromotionActionEvent args)
    {
        // Goobstation start
        if (args.Target == EntityUid.Invalid || args.Target == args.Performer)
            return;

        // Additional validation in case the target is no longer valid
        if (!HasComp<XenomorphComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-queen-promotion-invalid-target"), args.Performer);
            return;
        }

        if (!TryComp<XenomorphComponent>(args.Target, out var xenomorph))
            return;

        // Check if target is already a Praetorian or not in the whitelist
        if (xenomorph.Caste == "Praetorian" || !component.CasteWhitelist.Contains(xenomorph.Caste))
        {
            if (xenomorph.Caste == "Praetorian")
                _popup.PopupEntity(Loc.GetString("xenomorphs-queen-already-praetorian"), args.Performer);
            else
                _popup.PopupEntity(Loc.GetString("xenomorphs-queen-promotion-didnt-pass-whitelist"), args.Performer);
            return;
        }

        if (!_plasma.HasPlasma(uid, 500)
            || !_xenomorphEvolution.Evolve(args.Target, component.PromoteTo, component.EvolutionDelay))
            return;

        _plasma.ChangePlasmaAmount(uid, -500);
        args.Handled = true;
        // Goobstation end
    }
    // Dumont start
    public bool IsQueenAlive(bool lineageOnly = false)
    {
        var query = EntityQueryEnumerator<XenomorphQueenComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if ((!lineageOnly || HasComp<XenomorphLineageComponent>(uid)) && !_mobState.IsDead(uid))
                return true;
        }
        return false;
    }
    // Dumont end

}
