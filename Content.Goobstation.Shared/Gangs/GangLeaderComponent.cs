// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Gangs;

[RegisterComponent]
public sealed partial class GangLeaderComponent : Component
{
    [DataField]
    public EntityUid GangId;

    [DataField]
    public List<EntityUid> Members = new();
}
