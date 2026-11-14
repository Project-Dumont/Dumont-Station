// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Movement.Components;
using Content.Trauma.Common.Heretic;
using Content.Trauma.Shared.AudioMuffle;
using Content.Trauma.Shared.Heretic.Components.PathSpecific.Lock;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Systems.PathSpecific.Lock;

public abstract class SharedDigitalCamouflageSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        Subs.SubscribeWithRelay<DigitalCamouflageComponent, CanSeeOnCameraEvent>(OnCanSee, held: false);

        SubscribeLocalEvent<InventoryComponent, ExamineAttemptEvent>(OnExamineAttempt);
        SubscribeLocalEvent<DigitalCamouflageComponent, ExamineAttemptEvent>(OnExamineAttempt);
    }

    private void OnExamineAttempt(EntityUid uid, Component comp, ExamineAttemptEvent args)
    {
        if (!TryComp(args.Examiner, out RelayInputMoverComponent? relay) || !HasComp<AiEyeComponent>(relay.RelayEntity))
            return;

        var ev = new CanSeeOnCameraEvent(uid);
        RaiseLocalEvent(uid, ref ev);
        if (ev.Cancelled)
            args.Cancel();
    }

    private void OnCanSee(Entity<DigitalCamouflageComponent> ent, ref CanSeeOnCameraEvent args)
    {
        args.Cancelled = true;
    }
}
