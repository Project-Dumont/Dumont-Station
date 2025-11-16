// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Gangs;

[Serializable, NetSerializable]
public sealed partial class GangSprayDoAfterEvent : SimpleDoAfterEvent
{
    public NetEntity GangEntity;

    public GangSprayDoAfterEvent(NetEntity gangEntity)
    {
        GangEntity = gangEntity;
    }
}
