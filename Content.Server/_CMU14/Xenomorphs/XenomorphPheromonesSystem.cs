using Content.Shared._CMU14.Xenomorphs;
using Content.Server._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._CMU14.Xenomorphs.Pheromones;
using Content.Shared.Actions;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Server._CMU14.Xenomorphs;

public sealed partial class XenomorphPheromonesSystem : EntitySystem
{
    [Dependency] private XenomorphRoyalSystem _royal = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphPheromonesComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<XenomorphPheromonesComponent, XenomorphPheromonesActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphPheromonesComponent, XenoPheromonesChosenBuiMsg>(OnChosen);
        SubscribeLocalEvent<XenomorphPheromonesComponent, XenoPheromonesHelpButtonBuiMsg>(OnHelp);
        SubscribeLocalEvent<XenomorphPheromonesComponent, PlayerDetachedEvent>(OnDetached);
    }

    private void OnInit(Entity<XenomorphPheromonesComponent> ent, ref MapInitEvent args) => _actions.AddAction(ent, "ActionXenomorphPheromones");
    private void OnDetached(Entity<XenomorphPheromonesComponent> ent, ref PlayerDetachedEvent args) => _royal.ChoosePheromones(ent, "stopphero");

    private void OnAction(Entity<XenomorphPheromonesComponent> ent, ref XenomorphPheromonesActionEvent args)
    {
        if (args.Handled || !_mobs.IsAlive(ent)) return;
        _royal.ChoosePheromones(ent, "stopphero");
        args.Handled = _ui.TryOpenUi(ent.Owner, XenoPheromonesUI.Key, ent.Owner);
    }

    private void OnChosen(Entity<XenomorphPheromonesComponent> ent, ref XenoPheromonesChosenBuiMsg args)
    {
        if (args.Actor != ent.Owner || !Equals(args.UiKey, XenoPheromonesUI.Key) || !_mobs.IsAlive(ent) || !Enum.IsDefined(args.Pheromones)) return;
        if (_royal.ChoosePheromones(ent, args.Pheromones.ToString().ToLowerInvariant()))
            _ui.CloseUi(ent.Owner, XenoPheromonesUI.Key, ent.Owner);
    }

    private void OnHelp(Entity<XenomorphPheromonesComponent> ent, ref XenoPheromonesHelpButtonBuiMsg args)
    {
        if (args.Actor == ent.Owner && Equals(args.UiKey, XenoPheromonesUI.Key))
            _popup.PopupEntity(Loc.GetString("xenomorph-pheromones-help"), ent, ent);
    }
}
