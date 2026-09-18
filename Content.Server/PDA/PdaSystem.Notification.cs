// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.PDA;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;


namespace Content.Server.PDA
{
    public sealed partial class PdaSystem : SharedPdaSystem
    {
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly ILogManager _log = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;

        private ISawmill _sawmill = default!;

        private void OnPdaNotification(PdaNotificationEvent args)
        {
            _sawmill = _log.GetSawmill("pda_notification");

            if (!_proto.TryIndex<NotificationGroupPrototype>(args.Group, out var notiGroupProto)) {
                _sawmill.Error($"group {args.Group} does not exist'");
                return;
            }

            if (notiGroupProto.Access is null && notiGroupProto.AccessGroups is null) {
                PdaNotifyAll(args, notiGroupProto);
                return;
            }

            var Pdas = EntityQueryEnumerator<PdaComponent>();
            var amountNotified = 0;

            while (Pdas.MoveNext(out var uid, out var pdaComp)) {
                if (!IsValidPda(args, uid, pdaComp, out var accessLevels) || accessLevels is null)
                    continue;

                Entity<PdaComponent> pda = new(uid, pdaComp);

                if (notiGroupProto.Access is not null)
                    if (PdaNotifyByAccess(pda, notiGroupProto.Access, notiGroupProto.Exclude, accessLevels, args)) {
                        amountNotified++;
                        continue;
                    }

                if (notiGroupProto.AccessGroups is not null)
                    if (PdaNotifyByGroups(pda, notiGroupProto.AccessGroups, notiGroupProto.Exclude, accessLevels, args)) {
                        amountNotified++;
                        continue;
                    }
            }

            if (amountNotified == 0)
                _sawmill.Warning("Notified zero PDAs on last PdaNotificationEvent");

        }

        private bool PdaNotifyByAccess(
            Entity<PdaComponent> pda,
            HashSet<ProtoId<AccessLevelPrototype>> accessNoti,
            HashSet<ProtoId<AccessLevelPrototype>>? exclude,
            HashSet<ProtoId<AccessLevelPrototype>> accessLevels,
            PdaNotificationEvent args)
        {
            if (exclude is { } excludesAccess && accessLevels.Intersect(excludesAccess).Any())
                return false;

            if (!accessLevels.Intersect(accessNoti).Any())
                return false;

            NotifyPda(pda, args.Message, args.IsLoud);
            return true;
        }

        private bool PdaNotifyByGroups(
            Entity<PdaComponent> pda,
            HashSet<ProtoId<AccessGroupPrototype>> notiGroup,
            HashSet<ProtoId<AccessLevelPrototype>>? exclude,
            HashSet<ProtoId<AccessLevelPrototype>> accessLevels,
            PdaNotificationEvent args)
        {
            foreach (var accessGroupId in notiGroup) {
                if (!_proto.TryIndex<AccessGroupPrototype>(accessGroupId, out var accessGroup))
                    continue;

                if (PdaNotifyByAccess(pda, accessGroup.Tags, exclude, accessLevels, args))
                    return true;
            }

            return false;
        }

        public void PdaNotifyAll(PdaNotificationEvent args, NotificationGroupPrototype? proto = null) {
            var query = EntityQueryEnumerator<PdaComponent>();

            while (query.MoveNext(out var uid, out var comp)) {
                if (!IsValidPda(args, uid, comp, out var accessLevels) || accessLevels is null)
                    continue;

                if (proto is { } prototype &&
                    prototype.Exclude is { } exclusion &&
                    exclusion.Intersect(accessLevels).Any())
                    continue;

                NotifyPda((uid, comp), args.Message, args.IsLoud);
            }
        }

        public void NotifyPda(Entity<PdaComponent> ent, string message, bool isLoud = false) {
            _popup.PopupEntity(Loc.GetString("pda-new-notification"), ent.Owner, PopupType.Medium);

            if (isLoud)
                _ringer.RingerPlayRingtone(ent.Owner);

            ent.Comp.Notifications.Add(new Notification(_timing.CurTime, message));
            UpdatePdaUi(ent.Owner, ent.Comp);
        }

        private bool TryGetAccessLevels(PdaComponent pda, out HashSet<ProtoId<AccessLevelPrototype>>? accessLevels) {
            accessLevels = null;

            if (pda.IdSlot.Item is not { } idCardUid)
                return false;

            if (!TryComp<AccessComponent>(idCardUid, out var accessComp))
                return false;

            accessLevels = accessComp.Tags;
            return true;
        }

        private bool IsValidPda(PdaNotificationEvent args, EntityUid uid, PdaComponent pda, out HashSet<ProtoId<AccessLevelPrototype>>? accessLevels) {
            if (!TryGetAccessLevels(pda, out accessLevels))
                return false;

            if (args.Station is { } notifiedStation && notifiedStation != _station.GetOwningStation(uid))
                return false;

            return true;
        }
    }
}
