// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Mech.Components;
using Content.Server._Gabystation.CompactPod.Components;
using Content.Server.Body.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Content.Shared.Movement.Systems;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Content.Shared._Gabystation.CompactPod;

namespace Content.Server._Gabystation.CompactPod.Systems;

public sealed partial class CompactPodSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly SharedMechSystem _mech = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CompactPodComponent, MechEntryEvent>(OnEntryPod);
        SubscribeLocalEvent<CompactPodComponent, EntRemovedFromContainerMessage>(OnExitPod);

        SubscribeLocalEvent<CompactPodComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<CompactPodComponent, GetVerbsEvent<AlternativeVerb>>(OnPassenger);
        SubscribeLocalEvent<CompactPodComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CompactPodComponent, PodPassengerEntryEvent>(OnPassengerEntryPod);

        SubscribeLocalEvent<CompactPodPassengerComponent, InhaleLocationEvent>(OnInhale);
        SubscribeLocalEvent<CompactPodPassengerComponent, ExhaleLocationEvent>(OnExhale);

        SubscribeLocalEvent<CompactPodPassengerComponent, AtmosExposedGetAirEvent>(OnExpose);

        SubscribeLocalEvent<CompactPodComponent, DamageChangedEvent>(OnDestruction);
    }

    private void OnEntryPod(EntityUid uid, CompactPodComponent component, MechEntryEvent args)
    {
        if (args.Cancelled || !TryComp<JetpackComponent>(uid, out var jetpack))
            return;

        var xform = Transform(uid);
        if (xform.GridUid is not null)
        {
            ApplyGridMovement(uid);
        }
        else
        {
            ForcedJetpackActive(uid, jetpack);
        }

        _movementSpeedModifier.RefreshWeightlessModifiers(uid);
        args.Handled = true;
    }

    private void OnExitPod(EntityUid uid, CompactPodComponent component, ref EntRemovedFromContainerMessage args)
    {
        if (HasComp<CompactPodPassengerComponent>(args.Entity))
        {
            RemComp<CompactPodPassengerComponent>(args.Entity);
            return;
        }

        if (!TryComp<MechComponent>(uid, out var mech))
            return;

        if (mech.PilotSlot.ContainedEntity is not null)
            return;

        RemComp<ActiveJetpackComponent>(uid);
        RemComp<JetpackUserComponent>(uid);
        RemComp<CanMoveInAirComponent>(uid);

        if (TryComp<PhysicsComponent>(uid, out var physics))
            _physics.SetBodyStatus(uid, physics, BodyStatus.OnGround);

        _movementSpeedModifier.RefreshWeightlessModifiers(uid);

    }

    private void OnMapInit(EntityUid uid, CompactPodComponent component, MapInitEvent args) =>
        component.PassengerContainer = _container.EnsureContainer<Container>(uid, component.PassengerContainerId);

    private void OnPassenger(EntityUid uid, CompactPodComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (TryComp<MechComponent>(uid, out var mech) && mech.Broken)
            return;

        if (mech == null)
            return;

        if (component.PassengerContainer.ContainedEntities.Count < component.MaxPassengers)
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("mech-passager-verb-enter"),
                Act = () =>
                {
                    var doAfterArgs = new DoAfterArgs(EntityManager, args.User, TimeSpan.FromSeconds(mech.EntryDelay), new PodPassengerEntryEvent(), uid, target: uid)
                    {
                        BreakOnMove = true
                    };
                    _doAfter.TryStartDoAfter(doAfterArgs);
                }
            });
        }

        if (component.PassengerContainer.ContainedEntities.Count > 0)
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("mech-passager-verb-exit"),
                Priority = 1,
                Act = () =>
                {
                    foreach (var passenger in component.PassengerContainer.ContainedEntities.ToList())
                    {
                        _container.Remove(passenger, component.PassengerContainer);
                    }
                }
            });
        }
    }

    private void OnPassengerEntryPod(EntityUid uid, CompactPodComponent component, PodPassengerEntryEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!TryComp<MechComponent>(uid, out var mech))
            return;

        if (_whitelistSystem.IsWhitelistFail(mech.PilotWhitelist, args.User)
            || _whitelistSystem.IsBlacklistPass(mech.PilotBlacklist, args.User))
        {
            _popup.PopupEntity(Loc.GetString("mech-no-enter", ("item", uid)), args.User);
            return;
        }

        if (TryInsert(uid, args.User, component))
        {
            _actionBlocker.UpdateCanMove(uid);
            args.Handled = true;
        }
    }

    public bool TryInsert(EntityUid podUid, EntityUid passengerUid, CompactPodComponent component)
    {
        if (component.PassengerContainer.ContainedEntities.Count >= component.MaxPassengers)
        {
            _popup.PopupEntity(Loc.GetString("mech-passenger-full"), podUid, passengerUid);
            return false;
        }

        if (_container.Insert(passengerUid, component.PassengerContainer))
        {
            var passengerComp = EnsureComp<CompactPodPassengerComponent>(passengerUid);
            passengerComp.Pod = podUid;

            _popup.PopupEntity(Loc.GetString("mech-passenger-success"), podUid, passengerUid);
            return true;
        }

        return false;
    }

    private void OnParentChanged(EntityUid uid, CompactPodComponent comp, ref EntParentChangedMessage args)
    {
        if (Terminating(uid))
            return;

        if (!TryComp<JetpackComponent>(uid, out var jetpack))
            return;

        var xform = Transform(uid);

        if (xform.GridUid is not null)
            ApplyGridMovement(uid);
        else
        {
            // No espaço: Remove o auxílio de grid mas mantém o Jetpack
            RemComp<CanMoveInAirComponent>(uid);
            if (TryComp<MechComponent>(uid, out var pod) && !_mech.IsEmpty(pod))
                ForcedJetpackActive(uid, jetpack);
        }

        _movementSpeedModifier.RefreshWeightlessModifiers(uid);
    }

    private void ApplyGridMovement(EntityUid uid)
    {
        if (TryComp<PhysicsComponent>(uid, out var physics))
            _physics.SetBodyStatus(uid, physics, BodyStatus.InAir);

        EnsureComp<CanMoveInAirComponent>(uid);
    }

    private void OnInhale(EntityUid uid, CompactPodPassengerComponent component, InhaleLocationEvent args)
    {
        if (!TryComp<MechComponent>(component.Pod, out var mech)
            || !TryComp<MechAirComponent>(component.Pod, out var mechAir))
            return;

        if (mech.Airtight)
            args.Gas = mechAir.Air;
    }

    private void OnExhale(EntityUid uid, CompactPodPassengerComponent component, ExhaleLocationEvent args)
    {
        if (!TryComp<MechComponent>(component.Pod, out var mech)
            || !TryComp<MechAirComponent>(component.Pod, out var mechAir))
            return;

        if (mech.Airtight)
            args.Gas = mechAir.Air;
    }

    private void OnExpose(EntityUid uid, CompactPodPassengerComponent component, ref AtmosExposedGetAirEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<MechComponent>(component.Pod, out var mech))
            return;

        if (mech.Airtight && TryComp<MechAirComponent>(component.Pod, out var air))
        {
            args.Handled = true;
            args.Gas = air.Air;
            return;
        }

        args.Gas = _atmosphere.GetContainingMixture(component.Pod, excite: args.Excite);
        args.Handled = true;
    }

    private void OnDestruction(EntityUid uid, CompactPodComponent component, DamageChangedEvent args)
    {
        if (!TryComp<MechComponent>(uid, out var mech))
            return;

        if (mech.Broken || mech.Integrity <= 0)
        {
            EjectAllPassengers(uid, component);
        }
    }

    public void EjectAllPassengers(EntityUid uid, CompactPodComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var passengers = component.PassengerContainer.ContainedEntities.ToList();

        foreach (var passenger in passengers)
        {
            _container.Remove(passenger, component.PassengerContainer);

            _popup.PopupEntity(Loc.GetString("mech-passenger-ejected"), passenger, passenger);
        }
    }

    // We need to force jetpack "activation" on the entity itself because SharedJetpackSystem is unable to do this for the entity itself
    private void ForcedJetpackActive(EntityUid uid, JetpackComponent jetpack)
    {
        EnsureComp<ActiveJetpackComponent>(uid);
        var userComp = EnsureComp<JetpackUserComponent>(uid);

        userComp.Jetpack = uid;
        userComp.WeightlessAcceleration = jetpack.Acceleration;
        userComp.WeightlessModifier = jetpack.WeightlessModifier;
        userComp.WeightlessFriction = jetpack.Friction;
        userComp.WeightlessFrictionNoInput = jetpack.Friction;
    }
}
