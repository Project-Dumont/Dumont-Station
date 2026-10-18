// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Trauma.Common.CCVar;

[CVarDefs]
public sealed partial class TraumaCVars
{
    #region AudioMuffle

    /// <summary>
    /// Is audio muffle pathfinding behavior enabled?
    /// </summary>
    public static readonly CVarDef<bool> AudioMufflePathfinding =
        CVarDef.Create("trauma.audio_muffle_pathfinding", true, CVar.SERVER | CVar.REPLICATED);

    #endregion

    #region Station Traits

    /// <summary>
    /// Whether to enable station traits.
    /// Used to prevent tests failing for spawned gamerules.
    /// </summary>
    public static readonly CVarDef<bool> StationTraitsEnabled =
        CVarDef.Create("trauma.station_traits_enabled", true, CVar.SERVER);

    /// <summary>
    /// The offset for in-game date (the date will be server date + this amount of years).
    /// Displayed in the station report and PDAs.
    /// </summary>
    public static readonly CVarDef<int> YearOffset =
        CVarDef.Create("game.current_year_offset", 542, CVar.SERVERONLY); // Dumont

    #endregion

    #region EndCredits

    /// <summary>
    /// Whether to play the cool end credits.
    /// </summary>
    public static readonly CVarDef<bool> PlayMovieEndCredits =
        CVarDef.Create("trauma.play_credits", true, CVar.ARCHIVE | CVar.CLIENTONLY);

    #endregion
}
