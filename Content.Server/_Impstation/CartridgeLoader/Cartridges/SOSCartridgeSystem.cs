// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Radio.EntitySystems;
using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.PDA;
using Content.Shared.Popups;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._Impstation.CartridgeLoader.Cartridges;

public sealed class SOSCartridgeSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SOSCartridgeComponent, CartridgeActivatedEvent>(OnActivated);
    }

    private void OnActivated(EntityUid uid, SOSCartridgeComponent component, CartridgeActivatedEvent args)
    {
        if (!HasComp<PdaComponent>(args.Loader))
            return;

        if (component.NextMinimumTime < _timing.CurTime &&
            _container.TryGetContainer(args.Loader, SOSCartridgeComponent.PDAIdContainer, out var idContainer)) {

            //If theres nothing in id slot, send message anonymously
            if (idContainer.ContainedEntities.Count == 0)
                _radio.SendRadioMessage(uid, component.LocalizedDefaultName + " " + component.LocalizedHelpMessage, component.HelpChannel, uid);
            else
            {
                //Otherwise, send a message with the full name of every id in there
                foreach (var idCard in idContainer.ContainedEntities)
                {
                    if (!TryComp<IdCardComponent>(idCard, out var idCardComp))
                        continue;

                    _radio.SendRadioMessage(uid, (idCardComp.FullName ?? component.LocalizedDefaultName) + " " + component.LocalizedHelpMessage, component.HelpChannel, uid);
                }
            }

            // have to do this bullshit cuz timespan can't be constant
            component.NextMinimumTime = TimeSpan.FromSeconds(_timing.CurTime.TotalSeconds + SOSCartridgeComponent.TimeOut);
            // DeltaV - send feedback that you succeeded
            _popupSystem.PopupEntity(Loc.GetString("sos-message-sent-success"), uid, PopupType.Medium);
        }
        else {
            var seconds = component.NextMinimumTime.TotalSeconds - _timing.CurTime.TotalSeconds;
            seconds = Math.Round(seconds);
            _popupSystem.PopupEntity(Loc.GetString("sos-message-sent-cooldown", ("count", seconds)), uid, PopupType.MediumCaution);
        }
    }


}
