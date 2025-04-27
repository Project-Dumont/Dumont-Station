using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Verbs;
using Content.Shared.WashingMachine.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Shared.WashingMachine;

public sealed partial class WashingMachineSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedEntityStorageSystem _storage = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WashingMachineComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<WashingMachineComponent, ComponentRemove>(OnRemoved);

        SubscribeLocalEvent<WashingMachineComponent, BreakageEventArgs>(OnBreak);
        SubscribeLocalEvent<WashingMachineComponent, StorageOpenAttemptEvent>(OnStorageOpenAttempt);

        SubscribeLocalEvent<WashingMachineComponent, ActivateInWorldEvent>(OnActivateInWorld, before: [typeof(SharedEntityStorageSystem)]);
        SubscribeLocalEvent<WashingMachineComponent, GetVerbsEvent<ActivationVerb>>(OnGetVerbs);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<WashingMachineComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.WashingMachineState != WashingMachineState.Washing)
                continue;

            if (component.WashingFinished > _timing.CurTime)
                continue;

            component.WashingMachineState = WashingMachineState.Idle;
            DirtyField(uid, component, nameof(WashingMachineComponent.WashingMachineState));
            _appearance.SetData(uid, WashingMachineVisuals.State, component.WashingMachineState);

            HashSet<EntityUid> items = new();

            SharedEntityStorageComponent? entityStorageComp = null;
            if (_storage.ResolveStorage(uid, ref entityStorageComp))
                items = entityStorageComp.Contents.ContainedEntities.ToHashSet();

            if (component.WashingSoundStream != null)
                component.WashingSoundStream = _audio.Stop(component.WashingSoundStream);

            if (_net.IsServer)
                _audio.PlayPvs(component.FinishedSound, uid);

            var machineEv = new WashingMachineFinishedWashingEvent(items);
            RaiseLocalEvent(uid, machineEv);

            var itemEv = new WashingMachineWashedEvent(uid, items);
            foreach (var item in items)
                RaiseLocalEvent(item, itemEv);

            _storage.OpenStorage(uid);
        }
    }

    private void OnInit(Entity<WashingMachineComponent> ent, ref ComponentInit args)
    {
        _appearance.SetData(ent.Owner, WashingMachineVisuals.State, ent.Comp.WashingMachineState);
    }

    private void OnRemoved(Entity<WashingMachineComponent> ent, ref ComponentRemove args)
    {
        _audio.Stop(ent.Comp.WashingSoundStream);
    }

    private void OnBreak(Entity<WashingMachineComponent> ent, ref BreakageEventArgs args)
    {
        ent.Comp.WashingMachineState = WashingMachineState.Broken;
        DirtyField(ent.Owner, ent.Comp, nameof(WashingMachineComponent.WashingMachineState));
        _appearance.SetData(ent.Owner, WashingMachineVisuals.State, ent.Comp.WashingMachineState);
    }

    private void OnStorageOpenAttempt(Entity<WashingMachineComponent> ent, ref StorageOpenAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        args.Cancelled = ent.Comp.WashingMachineState != WashingMachineState.Idle;
    }

    private void OnActivateInWorld(Entity<WashingMachineComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (!CanActivate(ent))
            return;

        args.Handled = true;
        Activate(ent);
    }

    private void OnGetVerbs(Entity<WashingMachineComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanInteract || !args.CanComplexInteract)
            return;

        if (!CanActivate(ent))
            return;

        var verb = new ActivationVerb()
        {
            Text = Loc.GetString("washing-machine-start"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/Spare/poweronoff.svg.192dpi.png")),
            Act = () => Activate(ent)
        };

        args.Verbs.Add(verb);
    }

    private bool CanActivate(Entity<WashingMachineComponent> ent)
    {
        if (ent.Comp.WashingMachineState != WashingMachineState.Idle)
            return false;

        if (!_power.IsPowered(ent.Owner))
            return false;

        if (_storage.IsOpen(ent.Owner))
            return false;

        return true;
    }

    private void Activate(Entity<WashingMachineComponent> ent)
    {
        ent.Comp.WashingFinished = _timing.CurTime + ent.Comp.WashingTime;
        DirtyField(ent.Owner, ent.Comp, nameof(WashingMachineComponent.WashingFinished));

        ent.Comp.WashingMachineState = WashingMachineState.Washing;
        DirtyField(ent.Owner, ent.Comp, nameof(WashingMachineComponent.WashingMachineState));
        _appearance.SetData(ent.Owner, WashingMachineVisuals.State, ent.Comp.WashingMachineState);

        HashSet<EntityUid> items = new();

        SharedEntityStorageComponent? entityStorageComp = null;
        if (_storage.ResolveStorage(ent.Owner, ref entityStorageComp))
            items = entityStorageComp.Contents.ContainedEntities.ToHashSet();

        if (_net.IsServer)
        {
            var audio = _audio.PlayPvs(ent.Comp.WashingSound, ent.Owner);
            ent.Comp.WashingSoundStream = audio?.Entity;
        }

        var machineEv = new WashingMachineStartedWashingEvent(items);
        RaiseLocalEvent(ent.Owner, machineEv);

        var itemEv = new WashingMachineIsBeingWashed(ent.Owner, items);
        foreach (var item in items)
            RaiseLocalEvent(item, itemEv);
    }
}
