// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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

        public void OnPdaNotification(PdaNotificationEvent args)
        {
            _sawmill = _log.GetSawmill("pda_notification");

            if (!_proto.TryIndex<NotificationGroupPrototype>(args.Group, out var notiGroupProto)) {
                _sawmill.Error($"group '{args.Group} does not exist'");
                return;
            }

            if (notiGroupProto.Access is null && notiGroupProto.AccessGroups is null) {
                PdaNotifyAll(args);
                return;
            }

            var Pdas = EntityQueryEnumerator<PdaComponent>();
            var amountNotified = 0;

            while (Pdas.MoveNext(out var uid, out var pdaComp)) {
                if (pdaComp.IdSlot.Item is not { } idCardUid)
                    continue;

                if (!TryComp<AccessComponent>(idCardUid, out var accessComp))
                    continue;

                var accessLevels = accessComp.Tags;

                Entity<PdaComponent> pda = new(uid, pdaComp);

                if (notiGroupProto.Access is not null)
                    if (PdaNotifyByAccess(pda, notiGroupProto.Access, accessLevels, args)) {
                        amountNotified++;
                        continue;
                    }

                if (notiGroupProto.AccessGroups is not null)
                    if (PdaNotifyByGroups(pda, notiGroupProto.AccessGroups, accessLevels, args)) {
                        amountNotified++;
                        continue;
                    }
            }

            if (amountNotified == 0)
                _sawmill.Warning("Notified zero PDAs on last PdaNotificationEvent");

        }

        public bool PdaNotifyByAccess(
            Entity<PdaComponent> pda,
            HashSet<ProtoId<AccessLevelPrototype>> accessNoti,
            HashSet<ProtoId<AccessLevelPrototype>> accessLevels,
            PdaNotificationEvent args)
        {


            foreach (var accessSingular in accessNoti) {
                if (!accessLevels.Contains(accessSingular))
                    continue;

                NotifyPda(pda, args.Message, args.IsLoud);
                return true;
            }

            return false;
        }

        public bool PdaNotifyByGroups(
            Entity<PdaComponent> pda,
            HashSet<ProtoId<AccessGroupPrototype>> notiGroup,
            HashSet<ProtoId<AccessLevelPrototype>> accessLevels,
            PdaNotificationEvent args)
        {

            foreach (var accessGroupId in notiGroup) {
                if (!_proto.TryIndex<AccessGroupPrototype>(accessGroupId, out var accessGroup))
                    continue;

                if (PdaNotifyByAccess(pda, accessGroup.Tags, accessLevels, args))
                    return true;
            }

            return false;
        }

        public void PdaNotifyAll(PdaNotificationEvent args) {
            var query = EntityQueryEnumerator<PdaComponent>();

            while (query.MoveNext(out var uid, out var comp)) {
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
    }
}
