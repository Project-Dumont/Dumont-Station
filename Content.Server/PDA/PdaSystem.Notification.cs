// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.PDA;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Robust.Shared.Prototypes;


namespace Content.Server.PDA
{
    public sealed partial class PdaSystem : SharedPdaSystem
    {
        [Dependency] private readonly IPrototypeManager _proto = default!;

        public void OnPdaNotification(ref PdaNotificationEvent args)
        {
            if (!_proto.TryIndex<NotificationGroupPrototype>(args.Group, out var notiGroupProto))
                return;

            var Pdas = EntityQueryEnumerator<PdaComponent>();

            while (Pdas.MoveNext(out var uid, out var pdaComp)) {
                if (pdaComp.IdSlot.Item is not { } idCardUid)
                    continue;

                if (!TryComp<AccessComponent>(idCardUid, out var accessComp))
                    continue;

                if (notiGroupProto.Access is null || notiGroupProto.AccessGroups is null)
                    continue;

                var accessLevels = accessComp.Tags;

                Entity<PdaComponent> pda = new(uid, pdaComp);

                PdaNotifyByAccess(pda, notiGroupProto.Access, accessLevels, args);
                PdaNotifyByGroups(pda, notiGroupProto.AccessGroups, accessLevels, args);
            }

        }

        public void PdaNotifyByAccess(
            Entity<PdaComponent> pda,
            HashSet<ProtoId<AccessLevelPrototype>> accessNoti,
            HashSet<ProtoId<AccessLevelPrototype>> accessLevels,
            PdaNotificationEvent args)
        {
            foreach (var accessSingular in accessNoti) {
                if (!accessLevels.Contains(accessSingular))
                    continue;

                pda.Comp.Notifications.Add(args.Message);
                UpdatePdaUi(pda.Owner, pda.Comp);
            }
        }

        public void PdaNotifyByGroups(
            Entity<PdaComponent> pda,
            HashSet<ProtoId<AccessGroupPrototype>> notiGroup,
            HashSet<ProtoId<AccessLevelPrototype>> accessLevels,
            PdaNotificationEvent args)
        {

            foreach (var accessGroupId in notiGroup) {
                if (!_proto.TryIndex<AccessGroupPrototype>(accessGroupId, out var accessGroup))
                    continue;


                PdaNotifyByAccess(pda, accessGroup.Tags, accessLevels, args);
            }
        }
    }
}
