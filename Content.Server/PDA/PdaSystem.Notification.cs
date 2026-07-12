// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.PDA;

namespace Content.Server.PDA
{
    public sealed partial class PdaSystem : SharedPdaSystem
    {

        public void OnPdaNotification(Entity<PdaComponent> ent, ref PdaNotificationEvent args)
        {

        }
    }
}
