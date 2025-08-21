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
        if (component.IsBorg || args.SlotFlags != SlotFlags.MASK) // GabyStation - SecBorg Hailer
            return;

        _actionsSystem.AddAction(args.Equipee, ref component.HailActionEntity, component.HailerAction, args.Equipee);
    }
    private void OnGotUnequipped(EntityUid uid, HailerComponent component, GotUnequippedEvent args)
    {
        if (component.IsBorg || args.SlotFlags != SlotFlags.MASK) // GabyStation - SecBorg Hailer
            return;

        _actionsSystem.RemoveAction(args.Equipee, component.HailActionEntity);
    }
    string[] _sounds = [
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
    Dictionary<EntityUid, TimeSpan> _delays = new Dictionary<EntityUid, TimeSpan>();
    TimeSpan _fixed_delay = TimeSpan.FromSeconds(2);
    private void OnHail(EntityUid uid, ActionsComponent component, ref HailerActionEvent args)
    {
        if (args.Handled)
            return;
        // No hail spam check.
        if (_delays.ContainsKey(uid))
        {
            if (_timing.CurTime < _delays[uid])
            {
                return;
            }
        }
        int rInt = (int) _random.NextDouble(0, _sounds.Length);
        _audio.PlayPvs(_sounds[rInt], uid);
        _delays[uid] = _timing.CurTime.Add(_fixed_delay);

        var name = Name(uid);
        if (!TryComp<HailerComponent>(args.Performer, out var hailer) || !hailer.IsBorg) // GabyStation - SecBorg Hailer. Only borgs uid has HailerComponent
            name += "(SecMask)";

        _chat.TrySendInGameICMessage(uid, Loc.GetString("hail-" + rInt), InGameICChatType.Speak, ChatTransmitRange.GhostRangeLimit, nameOverride: name, checkRadioPrefix: false);
    }
}
