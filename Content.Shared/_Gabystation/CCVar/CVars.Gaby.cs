// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 joshepvodka <86210200+joshepvodka@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 joshepvodka <guilherme.ornel@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared._Gabystation.CCVar;

[CVarDefs]
public sealed partial class GabyCVars
{
    /// <summary>
    /// Discord Webhooks
    /// </summary>
    public static readonly CVarDef<string> BanDiscordWebhook =
        CVarDef.Create("discord.ban_webhook", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Enables alternate job titles for players.
    /// </summary>
    public static readonly CVarDef<bool> ICAlternateJobTitlesEnable =
        CVarDef.Create("ic.alternate_job_titles_enable", true, CVar.SERVER | CVar.REPLICATED);

    // Enshittificar Cirurgias e Cia

    /// <summary>
    /// Poison damage applied per surgery step when the surgeon lacks proper PPE.
    /// Triggers when missing gloves or mask and not sanitized.
    /// </summary>
    public static readonly CVarDef<float> SurgerySepsisEquipmentPenalty =
        CVarDef.Create("gaby.surgery.sepsis_equipment_penalty", 5f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Poison damage applied per surgery step when operating outside proper surgical tables.
    /// </summary>
    public static readonly CVarDef<float> SurgerySepsisLocationPenalty =
        CVarDef.Create("gaby.surgery.sepsis_location_penalty", 5f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Poison damage applied per surgery step for each unsanitazed person around surgery.
    /// </summary>
    public static readonly CVarDef<float> SurgerySepsisCrowdingPenalty =
        CVarDef.Create("gaby.surgery.sepsis_crowding_penalty", 5f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Range in tiles to check for crowding around surgery sites.
    /// Only living entities within this range count toward crowding penalties.
    /// </summary>
    public static readonly CVarDef<float> SurgeryCrowdingCheckRange =
        CVarDef.Create("gaby.surgery.crowding_check_range", 5f, CVar.SERVER | CVar.REPLICATED);

    #region Server Economy
    public static readonly CVarDef<float> SCurrencyRotationCooldown =
        CVarDef.Create("servercurrency.rotation_cooldown", 7200f, CVar.SERVER);

    public static readonly CVarDef<int> SCurrencyStoreTokens =
        CVarDef.Create("servercurrency.tokens_per_rotation", 4, CVar.SERVER);

    #endregion
}
