// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.PDA;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Notification;

namespace Content.Server.PDA
{
    public sealed partial class PdaSystem : SharedPdaSystem
    {
        [Dependency] private IPrototypeManager _proto = default!;
        [Dependency] private ILogManager _log = default!;
        [Dependency] private SharedPopupSystem _popup = default!;

        private ISawmill _sawmill = default!;

        private void OnNotification(NotificationEvent ev)
        {
            var pdas = EntityQueryEnumerator<PdaComponent, NotificationReceiverComponent>();

            if (!_proto.TryIndex<NotificationGroupPrototype>(ev.Group, out var notiGroupProto)) {
                _sawmill.Error($"group {ev.Group} does not exist'");
                return;
            }

            if (notiGroupProto.Access is null && notiGroupProto.AccessGroups is null) {
                NotifyAll(ev);
                return;
            }
            
            var amountNotified = 0;
            while (pdas.MoveNext(out var uid, out var pdaComp, out var receiverComp))
                amountNotified += HandleNotification((uid, pdaComp), receiverComp, ev, notiGroupProto) ? 1 : 0;

            _sawmill = _log.GetSawmill("pda_notification");
            if (amountNotified > 0)
                _sawmill.Warning("Notified zero PDAs on last NotificationEvent");

        }

        /// <summary>
        /// Handles a PDA being notified, taking into account notification groups and such.
        /// </summary>
        /// <param name="pda">Pda to notify</param>
        /// <param name="args">Arguments of the event, used for the groups</param>
        /// <returnsWhether the PDA was notified></returns>
        private bool HandleNotification(
        Entity<PdaComponent> pda, 
        NotificationReceiverComponent receiveComp, 
        NotificationEvent args, 
        NotificationGroupPrototype notiGroupProto)
        {
            if (!IsValidNotification(args.Station, pda.Owner, pda.Comp, out var accessLevels) || accessLevels is null)
                return false;

            if (notiGroupProto.Access is not null)
                if (NotifyByAccess(pda, notiGroupProto.Access, notiGroupProto.Exclude, accessLevels, args))
                    return true;

            if (notiGroupProto.AccessGroups is not null)
                if (NotifyByGroups(pda, notiGroupProto.AccessGroups, notiGroupProto.Exclude, accessLevels, args))
                    return true;
                
            return false;

        }

        private bool NotifyByAccess(
            Entity<PdaComponent> pda,
            HashSet<ProtoId<AccessLevelPrototype>> accessNoti,
            HashSet<ProtoId<AccessLevelPrototype>>? exclude,
            HashSet<ProtoId<AccessLevelPrototype>> accessLevels,
            NotificationEvent args)
        {
            if (exclude is { } excludesAccess && accessLevels.Intersect(excludesAccess).Any())
                return false;

            if (!accessLevels.Intersect(accessNoti).Any())
                return false;

            Notify(pda, args.Message, args.IsLoud);
            return true;
        }

        private bool NotifyByGroups(
            Entity<PdaComponent> pda,
            HashSet<ProtoId<AccessGroupPrototype>> notiGroup,
            HashSet<ProtoId<AccessLevelPrototype>>? exclude,
            HashSet<ProtoId<AccessLevelPrototype>> accessLevels,
            NotificationEvent args)
        {
            foreach (var accessGroupId in notiGroup) {
                if (!_proto.TryIndex<AccessGroupPrototype>(accessGroupId, out var accessGroup))
                    continue;

                if (NotifyByAccess(pda, accessGroup.Tags, exclude, accessLevels, args))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Notifies all PDA, regardless of their notification groups.
        /// </summary>
        /// <param name="args">Event that holds all the information necessary for the notification.</param>
        /// <param name="proto">Notification group, used for optional exclusion, leave null for no filtering</param>
        private void NotifyAll(NotificationEvent args, NotificationGroupPrototype? proto = null) {
            var query = EntityQueryEnumerator<PdaComponent>();

            while (query.MoveNext(out var uid, out var comp)) {
                if (!IsValidNotification(args.Station, uid, comp, out var accessLevels) || accessLevels is null)
                    continue;

                if (proto is { } prototype &&
                    prototype.Exclude is { } exclusion &&
                    exclusion.Intersect(accessLevels).Any())
                    continue;

                Notify((uid, comp), args.Message, args.IsLoud);
            }
        }

        public void Notify(Entity<PdaComponent> pda, string message, bool isLoud = false) {
            if (!TryComp<NotificationReceiverComponent>(pda.Owner, out var receiverComp))
                return;

            if (!IsPdaAttachedToPlayer(pda))
                _popup.PopupEntity(Loc.GetString("pda-new-notification"), pda.Owner, PopupType.Medium);

            if (receiverComp.Setting == NotificationSetting.Loud || (receiverComp.Setting == NotificationSetting.Normal && isLoud))
                _ringer.RingerPlayRingtone(pda.Owner, false);

            receiverComp.Notifications.Add(new NotificationMessage(_timing.CurTime, message));
            Dirty(pda);
            UpdatePdaUi(pda.Owner, pda.Comp);
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

        /// <summary>
        /// Checks whether a PDA is valid for receiving a notification at all.
        /// </summary>
        /// <param name="station">Optional station filtering</param>
        /// <param name="uid"Pda uid></param>
        /// <param name="pda"Pda component></param>
        /// <param name="accessLevels">The accesslevels acquired from the PDA, used for checking Notification Groups</param>
        /// <returns></returns>
        private bool IsValidNotification(EntityUid? station, EntityUid uid, PdaComponent pda, out HashSet<ProtoId<AccessLevelPrototype>>? accessLevels) {
            if (!TryGetAccessLevels(pda, out accessLevels))
                return false;

            if (station is { } notifiedStation && notifiedStation != _station.GetOwningStation(uid))
                return false;

            return true;
        }
    }
}
