// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.PDA;

namespace Content.Server.PDA
{
    public sealed partial class PdaSystem : SharedPdaSystem
    {
        [Dependency] private readonly IPrototypeManager _proto = default!;

        public void OnPdaNotification(Entity<PdaComponent> ent, ref PdaNotificationEvent args)
        {
            var Pdas = EntityQueryEnumerator<PdaComponent>();

            while (Pdas.MoveNext(out uid, out pdaComp)) {
                if (pdaComp.IdSlot.Item is not { } idCardUid)
                    continue;

                if (!TryComp<AccessComponent>(idCardUid, out var accessComp))
                    continue;

                var accessLevels = accessComp.Tags;

                PdaNotifyByAccess(args.Group.Access, accessLevels, pdaComp, args);
                PdaNotifyByGroups(args.Group.AccessGroup, accessLevels, pdaComp, args);
            }

        }

        public void PdaNotifyByAccess(
            IEnumerable access,
            HashSet<ProtoId<AccessLevelPrototype>> accessLevels,
            PdaComponent pdaComp,
            PdaNotificationEvent args)
        {
            foreach (var accessSingular in access) {
                if (accessLevels.Contains(accessSingular)) {
                    pdaComp.Notifications,Add(args.Message);
                    UpdateState();
                }
            }
        }

        public void PdaNotifyByGroups(
            IEnumerable group,
            HashSet<ProtoId<AccessLevelPrototype>> accessLevels,
            PdaComponent pdaComp,
            PdaNotificationEvent args)
        ) {
            foreach (var accessGroup in group) {
                PdaNotifyByAccess(accessGroup.Tags, accessLevels, pdaComp, args);
            }
        }
    }
}
