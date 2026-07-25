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
            if (!_proto.TryIndex<NotificationGroupPrototype>(args.Group, out var notiGroupProto))
                return;

            if (notiGroupProto.Access is null && notiGroupProto.AccessGroups is null) {
                PdaNotifyAll(args);
                return;
            }

            var Pdas = EntityQueryEnumerator<PdaComponent>();

            while (Pdas.MoveNext(out var uid, out var pdaComp)) {
                if (pdaComp.IdSlot.Item is not { } idCardUid)
                    continue;

                if (!TryComp<AccessComponent>(idCardUid, out var accessComp))
                    continue;

                var accessLevels = accessComp.Tags;

                Entity<PdaComponent> pda = new(uid, pdaComp);

                if (notiGroupProto.Access is not null)
                    if (PdaNotifyByAccess(pda, notiGroupProto.Access, accessLevels, args))
                        continue;

                if (notiGroupProto.AccessGroups is not null)
                    if (PdaNotifyByGroups(pda, notiGroupProto.AccessGroups, accessLevels, args))
                        continue;
            }

        }

        public bool PdaNotifyByAccess(
            Entity<PdaComponent> pda,
            HashSet<ProtoId<AccessLevelPrototype>> accessNoti,
            HashSet<ProtoId<AccessLevelPrototype>> accessLevels,
            PdaNotificationEvent args)
        {
            _sawmill = _log.GetSawmill("notification");

            foreach (var accessSingular in accessNoti) {
                if (!accessLevels.Contains(accessSingular))
                    continue;


                NotifyPda(pda, args);
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
                NotifyPda((uid, comp), args);
            }
        }

        public void NotifyPda(Entity<PdaComponent> ent, PdaNotificationEvent args) {
            _popup.PopupEntity(Loc.GetString("pda-new-notification"), ent.Owner);

            if (args.IsLoud)
                _ringer.RingerPlayRingtone(ent.Owner);

            ent.Comp.Notifications.Add(args.Message);
            UpdatePdaUi(ent.Owner, ent.Comp);
        }
    }
}
