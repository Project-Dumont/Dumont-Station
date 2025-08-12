// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
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
    /// Quantidade de veneno causado por passo ao fazer uma cirurgia sem luva ou sem máscara.
    /// </summary>
    public static readonly CVarDef<float> SurgeryWithoutEquipmentDamage =
        CVarDef.Create("gaby.surgery.surgery_without_equipment_damage", 5f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Quantidade de veneno causado por passo ao fazer uma cirurgia fora da mesa de operação.
    /// </summary>
    public static readonly CVarDef<float> SurgeryOffTableDamage =
        CVarDef.Create("gaby.surgery.surgery_off_table_damage", 5f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Quantidade de veneno causado por passo e por ser vivo ao fazer uma cirurgia além da lotação máxima.
    /// </summary>
    public static readonly CVarDef<float> SurgeryMaxLotationDamage =
        CVarDef.Create("gaby.surgery.surgery_max_lotation_damage", 5f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Quantidade máxima de seres vivos permitida ao redor de uma cirurgia (sem contar o paciente).
    /// </summary>
    public static readonly CVarDef<int> SurgeryMaxLotation =
        CVarDef.Create("gaby.surgery.surgery_max_lotation", 3, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Distância em tiles que será procurado entidades pra lotação máxima.
    /// </summary>
    public static readonly CVarDef<float> SurgeryMaxLotationDistance =
        CVarDef.Create("gaby.surgery.surgery_max_lotation_range", 5f, CVar.SERVER | CVar.REPLICATED);

}
