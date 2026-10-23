// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared._ES.CCVar;

[CVarDefs]
public sealed class ESCCVars
{
    /// <summary>
    /// If the player wants to watch the death cutscene or not. If disabled, they just ghost immediately,
    /// no fade, no music. Replicated so the server knows it needs to skip the cutscene for this player.
    /// </summary>
    public static readonly CVarDef<bool> DeathCutscene =
        CVarDef.Create("accessibility.death_cutscene", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);
}
