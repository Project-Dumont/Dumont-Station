using Content.Shared._CMU14.Xenomorphs;
using Content.Server._CMU14.Xenomorphs;
using System.Numerics;
using Content.Server._White.Xenomorphs.Plasma;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Robust.Shared.Prototypes;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Radio.Components;
using Robust.Shared.Timing;
using Robust.Shared.Map;

namespace Content.Server._Dumont.Xenomorphs;

[RegisterComponent]
public sealed partial class XenomorphRadioJammerComponent : Component
{
    [DataField] public float Range = 6;
    [DataField] public int ActivationCost = 25;
    [DataField] public int PlasmaPerSecond = 5;
    public EntityUid? Emitter;
    public EntityUid? Action;
    public TimeSpan NextPayment;
}

/// <summary>
/// Powers the existing radio jammer with plasma. The emitter is a child of the xeno,
/// keeping the item's battery interaction and examination off the mob itself.
/// </summary>
public sealed partial class XenomorphRadioJammerSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private PlasmaSystem _plasma = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private XenomorphHiveRanksSystem _ranks = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphRadioJammerComponent, MapInitEvent>(OnInit,
            after: new[] { typeof(Content.Server.Body.Systems.BodySystem) });
        SubscribeLocalEvent<XenomorphRadioJammerComponent, XenomorphRadioJammerActionEvent>(OnAction);
        SubscribeLocalEvent<XenomorphRadioJammerComponent, MobStateChangedEvent>(OnState);
        SubscribeLocalEvent<XenomorphRadioJammerComponent, ComponentShutdown>(OnShutdown);
    }

    private static readonly EntProtoId JammerAction = "ActionXenomorphRadioJammer";

    private void OnInit(Entity<XenomorphRadioJammerComponent> ent, ref MapInitEvent args)
    {
        if (!HasComp<XenomorphLineageComponent>(ent) || !TryComp<XenomorphComponent>(ent, out var xeno)
            || _ranks.Tier(xeno.Caste) is not (1 or 2)
            || !TryComp<PlasmaVesselComponent>(ent, out var plasma) || plasma.MaxPlasma <= 0)
        {
            RemComp<XenomorphRadioJammerComponent>(ent);
            return;
        }
        var jammer = ent.Comp;
        _actions.AddAction(ent, ref jammer.Action, JammerAction);
    }

    private void OnAction(Entity<XenomorphRadioJammerComponent> ent, ref XenomorphRadioJammerActionEvent args)
    {
        if (args.Handled) return;
        if (ent.Comp.Emitter != null)
        {
            Stop(ent);
            args.Handled = true;
            return;
        }
        if (!_mobs.IsAlive(ent) || !_blocker.CanConsciouslyPerformAction(ent)) return;
        if (!_plasma.ChangePlasmaAmount(ent, -ent.Comp.ActivationCost))
        {
            _popup.PopupEntity(Loc.GetString("xenomorph-jammer-no-plasma"), ent, ent);
            return;
        }

        var emitter = Spawn(null, Transform(ent).Coordinates);
        _transform.SetCoordinates(emitter, new EntityCoordinates(ent, Vector2.Zero));
        var radio = AddComp<RadioJammerComponent>(emitter);
        radio.Settings = new[] { new RadioJammerComponent.RadioJamSetting { Range = ent.Comp.Range, Wattage = 0 } };
        radio.SelectedPowerLevel = 0;
        Dirty(emitter, radio);
        AddComp<ActiveRadioJammerComponent>(emitter);
        ent.Comp.Emitter = emitter;
        ent.Comp.NextPayment = _timing.CurTime + TimeSpan.FromSeconds(1);
        _actions.SetToggled(ent.Comp.Action, true);
        args.Handled = true;
    }

    private void Stop(Entity<XenomorphRadioJammerComponent> ent)
    {
        if (ent.Comp.Emitter is { } emitter && !TerminatingOrDeleted(emitter))
        {
            RemComp<ActiveRadioJammerComponent>(emitter);
            QueueDel(emitter);
        }
        ent.Comp.Emitter = null;
        _actions.SetToggled(ent.Comp.Action, false);
    }

    private void OnState(Entity<XenomorphRadioJammerComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive) Stop(ent);
    }

    private void OnShutdown(Entity<XenomorphRadioJammerComponent> ent, ref ComponentShutdown args)
    {
        Stop(ent);
        _actions.RemoveAction(ent.Comp.Action);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenomorphRadioJammerComponent>();
        while (query.MoveNext(out var uid, out var jammer))
        {
            if (jammer.Emitter == null || _timing.CurTime < jammer.NextPayment) continue;
            if (TerminatingOrDeleted(jammer.Emitter.Value) || !_mobs.IsAlive(uid)
                || !_blocker.CanConsciouslyPerformAction(uid)
                || !_plasma.ChangePlasmaAmount(uid, -jammer.PlasmaPerSecond))
            {
                Stop((uid, jammer));
                continue;
            }
            jammer.NextPayment += TimeSpan.FromSeconds(1);
        }
    }
}





