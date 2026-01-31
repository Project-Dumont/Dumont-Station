using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Actions;
using Content.Shared.Genetics;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Genetics;

public sealed class CryokinesisGenSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CryokinesisGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CryokinesisGenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CryokinesisGenComponent, CryokinesisActionEvent>(OnCryokinesis);
    }

    private void OnInit(Entity<CryokinesisGenComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ActionEntity = _actions.AddAction(ent, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<CryokinesisGenComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.ActionEntity != null)
            _actions.RemoveAction(ent.Comp.ActionEntity);
    }

    private void OnCryokinesis(Entity<CryokinesisGenComponent> ent, ref CryokinesisActionEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (HasComp<ColdResistanceGenComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("cryokinesis-target-resistant", ("target", target)), ent, ent);
            return;
        }

        if (!TryComp<TemperatureComponent>(target, out var tempComp))
        {
            _popup.PopupEntity(Loc.GetString("cryokinesis-target-immune", ("target", target)), ent, ent);
            return;
        }

        var heatCapacity = _temperature.GetHeatCapacity(target);
        var energyToRemove = heatCapacity * ent.Comp.TemperatureDelta;

        _temperature.ChangeHeat(target, -energyToRemove, ignoreHeatResistance: true, tempComp);
        _popup.PopupEntity(Loc.GetString("cryokinesis-activated", ("target", target)), ent, ent);
        args.Handled = true;
    }
}
