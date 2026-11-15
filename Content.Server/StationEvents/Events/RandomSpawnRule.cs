// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared._Starlight.Lock;
using Content.Server.Pinpointer;
using Content.Server.Radio.EntitySystems;
using Robust.Shared.Utility;

namespace Content.Server.StationEvents.Events;

public sealed partial class RandomSpawnRule : StationEventSystem<RandomSpawnRuleComponent>
{
    [Dependency] private NavMapSystem _navMap = default!; // Moffstation
    [Dependency] private RadioSystem _radio = default!; // Moffstation

    protected override void Started(EntityUid uid, RandomSpawnRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        if (TryFindRandomTile(out _, out _, out _, out var coords))
        {
            Sawmill.Info($"Spawning {comp.Prototype} at {coords}");
            var ent = Spawn(comp.Prototype, coords);

            // Moffstation
            if (comp.RadioMessage is {} radioMessage)
            {
                var message = Loc.GetString(radioMessage.Message,
                    ("location", FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString(ent))));
                if (TryComp<DigitalLockComponent>(ent, out var digitalLock))
                {
                    if (digitalLock.Code.Length == 0)
                    {
                        for (var i = 0; i < digitalLock.MaxCodeLength; i++)
                            digitalLock.Code += RobustRandom.Next(0, 10).ToString();
                    }

                    message += " " + Loc.GetString("dead-drop-code-announcement", ("code", digitalLock.Code));
                }

                var sender = radioMessage.Sender is {} prototype ? Spawn(prototype, coords) : ent;
                _radio.SendRadioMessage(sender, message, radioMessage.Channel, sender);
            }
        }
    }
}