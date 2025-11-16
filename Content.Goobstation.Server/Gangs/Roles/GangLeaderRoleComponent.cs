// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Gangs.Roles;

[RegisterComponent]
public sealed partial class GangLeaderRoleComponent : Component
{
    [DataField]
    public EntityUid GangId;
}
