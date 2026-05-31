// SPDX-FileCopyrightText: 2024 Nikita Rαmses Abdoelrahman <ramses@starwolves.io>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Tim <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Hailer;
using Content.Server.Chat.Systems;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Chat;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Hailer;

public sealed class HailerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    private readonly string[] _sounds = [
        "/Audio/_Goobstation/Hailer/asshole.ogg",
        "/Audio/_Goobstation/Hailer/bash.ogg",
        "/Audio/_Goobstation/Hailer/bobby.ogg",
        "/Audio/_Goobstation/Hailer/compliance.ogg",
        "/Audio/_Goobstation/Hailer/dontmove.ogg",
        "/Audio/_Goobstation/Hailer/dredd.ogg",
        "/Audio/_Goobstation/Hailer/floor.ogg",
        "/Audio/_Goobstation/Hailer/freeze.ogg",
        "/Audio/_Goobstation/Hailer/halt.ogg",
    ];

    private readonly Dictionary<EntityUid, TimeSpan> _delays = new();
    private readonly TimeSpan _fixed_delay = TimeSpan.FromSeconds(2);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HailerComponent, ComponentStartup>(OnStartup); // GabyStation - SecBorg Hailer
        SubscribeLocalEvent<ActionsComponent, HailerActionEvent>(OnHail);
        SubscribeLocalEvent<HailerComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<HailerComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnStartup(EntityUid uid, HailerComponent component, ComponentStartup args)
    {
        if (!component.IsBorg) // GabyStation - SecBorg Hailer
            return;

        _actionsSystem.AddAction(uid, ref component.HailActionEntity, component.HailerAction, uid);
    }

    private void OnGotEquipped(EntityUid uid, HailerComponent component, GotEquippedEvent args)
    {
        if (component.IsBorg) // GabyStation - SecBorg Hailer
            return;

        _actionsSystem.AddAction(args.Equipee, ref component.HailActionEntity, component.HailerAction, args.Equipee);
    }

    private void OnGotUnequipped(EntityUid uid, HailerComponent component, GotUnequippedEvent args)
    {
        if (component.IsBorg) // GabyStation - SecBorg Hailer
            return;

        _actionsSystem.RemoveAction(args.Equipee, component.HailActionEntity);
    }

    private void OnHail(EntityUid uid, ActionsComponent actions, ref HailerActionEvent args)
    {
        if (args.Handled)
            return;

        // No hail spam check.
        var performer = uid;
        HailerComponent? hailerComponent = null;
        EntityUid? hailerEntity = null;

        if (TryComp<HailerComponent>(performer, out var borgComp) && borgComp.IsBorg)
        {
            hailerComponent = borgComp;
            hailerEntity = performer;
        }
        else if (_inventory.TryGetSlots(performer, out var slotDefinitions))
        {
            foreach (var slot in slotDefinitions)
            {
                if (_inventory.TryGetSlotEntity(performer, slot.Name, out var item) && 
                    TryComp<HailerComponent>(item, out var comp))
            {
                    hailerComponent = comp;
                    hailerEntity = item;
                    break;
                }
            }
        }

        if (hailerComponent == null || hailerEntity == null)
            return;

        EnsureComp<UseDelayComponent>(hailerEntity.Value, out var useDelay);
        var hailerEnt = new Entity<UseDelayComponent>(hailerEntity.Value, useDelay);

        if (_useDelay.IsDelayed(hailerEnt.AsNullable()))
            return;

        _useDelay.SetLength(hailerEnt.AsNullable(), TimeSpan.FromSeconds(hailerComponent.CooldownDuration));
        _useDelay.TryResetDelay(hailerEnt);

        if (!string.IsNullOrEmpty(hailerComponent.HailMessage))
        {
            if (hailerComponent.HailSound != null)
            {
                _audio.PlayPvs(hailerComponent.HailSound, performer);
            }

            _chat.TrySendInGameICMessage(performer, hailerComponent.HailMessage, InGameICChatType.Speak, false);
        }
        else
        {
            int rInt = (int)_random.NextDouble(0, _sounds.Length);
            _audio.PlayPvs(_sounds[rInt], performer);

            var name = Name(performer);
            if (!hailerComponent.IsBorg) // GabyStation - SecBorg Hailer. Only borgs uid has HailerComponent
                name += "(SecMask)";

            _chat.TrySendInGameICMessage(performer, Loc.GetString("hail-" + rInt), InGameICChatType.Speak, ChatTransmitRange.GhostRangeLimit, nameOverride: name, checkRadioPrefix: false);
        }

        args.Handled = true;
    }
}
