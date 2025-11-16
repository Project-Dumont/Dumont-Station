// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Gangs;

[RegisterComponent]
public sealed partial class PendingGangHandshakeComponent : Component
{
    [DataField]
    public EntityUid Offerer;

    [DataField]
    public TimeSpan ExpiryTime;
}
