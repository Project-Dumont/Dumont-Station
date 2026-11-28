// SPDX-FileCopyrightText: 2025-2026 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2017-2026 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Content.Shared.Radio;
using Content.Shared.Radio.Components;

namespace Content.Server._ES.Sabotage;

public sealed partial class TraitorSabotageSystem
{
    partial void InitializeTelecom()
    {
        SubscribeLocalEvent<TelecomServerComponent, SabotageEffectEvent>(OnTelecomSabotage);
    }

    private void OnTelecomSabotage(Entity<TelecomServerComponent> ent, ref SabotageEffectEvent args)
    {
        if (args.Handled || !TryComp<SabotageTargetComponent>(ent, out var sabotage))
            return;

        sabotage.Sabotaged = true;
        _chat.DispatchStationAnnouncement(ent, Loc.GetString("es-sabotage-telecom-announcement"));
        args.Handled = true;
    }

    public string ScrambleRadio(string message, EntityUid radioSource, RadioChannelPrototype channel)
    {
        var station = _station.GetOwningStation(radioSource);
        if (station == null)
            return message;

        // The local telecom servers relay individual channels, unlike ES's central processor.
        var query = EntityQueryEnumerator<TelecomServerComponent, SabotageTargetComponent, EncryptionKeyHolderComponent>();
        while (query.MoveNext(out var uid, out _, out var sabotage, out var keys))
        {
            if (sabotage.Sabotaged && IsOperational(uid) && keys.Channels.Contains(channel.ID)
                && _station.GetOwningStation(uid) == station)
                return DistortMessage(message, 0.6f);
        }
        return message;
    }
}
