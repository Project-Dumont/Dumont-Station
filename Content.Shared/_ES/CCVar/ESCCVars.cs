// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared._ES.CCVar;

[CVarDefs]
public sealed class ESCCVars
{
    /// <summary>
    /// If the player wanna watch the death cutscene or no. If disabled, they just ghost imediatly,
        /// no fade, no musics. Replicated so the server know it need skip the cutscene for this players.
        /// why did it have to give so many errors bruh
    /// </summary>
    public static readonly CVarDef<bool> DeathCutscene =
        CVarDef.Create("accessibility.death_cutscene", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);
}
