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
            if (ent.Comp.IdSlot.Item is not { } idCardUid)
                return;

            if (!TryComp<AccessComponent>(idCardUid, out var accessComp))
                return;

            var accessLevels = accessComp.Tags;

            PdaNotifyByAccess(args.Group.Access, accessLevels, ent.Comp, args);
            PdaNotifyByGroups(args.Group.AccessGroup, accessLevels, ent.Comp, args);
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
