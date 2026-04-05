// SPDX-FileCopyrightText: 2026 Goob Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Goobstation.Shared.MisandryBox.Thunderdome;

[CVarDefs]
public sealed class ThunderdomeCVars
{
    public static readonly CVarDef<bool> ThunderdomeEnabled =
        CVarDef.Create("thunderdome.enabled", true, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<bool> ThunderdomeRefill =
        CVarDef.Create("thunderdome.refill", true, CVar.SERVER | CVar.REPLICATED);
}
