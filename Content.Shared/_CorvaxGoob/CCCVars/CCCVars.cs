// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared._CorvaxGoob.CCCVars;

/// <summary>
///     Corvax modules console variables
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming
public sealed class CCCVars
{
    /// <summary>
    /// Offer item.
    /// </summary>
    public static readonly CVarDef<bool> OfferModeIndicatorsPointShow =
        CVarDef.Create("hud.offer_mode_indicators_point_show", true, CVar.ARCHIVE | CVar.CLIENTONLY);

    public static readonly CVarDef<bool> PhotoPlayTimeRequire =
        CVarDef.Create("photo.playtime_require", true, CVar.SERVERONLY);

    public static readonly CVarDef<float> PhotoPlayTimeHours =
        CVarDef.Create("photo.playtime_require_time", 24f, CVar.SERVERONLY);
}
