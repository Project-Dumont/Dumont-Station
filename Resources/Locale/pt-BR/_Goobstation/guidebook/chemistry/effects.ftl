# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

reagent-effect-guidebook-deal-stamina-damage =
    { $chance ->
        [1] { $deltasign ->
                [1] Causa
                *[-1] Cura
            }
        *[other]
            { $deltasign ->
                [1] causar
                *[-1] curar
            }
    } { $amount } { $immediate ->
                    [true] dano imediato
                    *[false] dano ao longo do tempo
                  } de stamina

reagent-effect-guidebook-stealth-entities = Camufla criaturas vivas próximas.

reagent-effect-guidebook-change-faction = Altera a facção da criatura para {$faction}.

reagent-effect-guidebook-mutate-plants-nearby = Muta aleatoriamente plantas próximas.

reagent-effect-guidebook-dnascramble = Embaralha o DNA da pessoa.

reagent-effect-guidebook-change-species = Transforma o alvo em {$species}

reagent-effect-guidebook-sex-change = Troca o gênero da pessoa

reagent-effect-guidebook-immunity-modifier =
    { $chance ->
        [1] Modifica
        *[other] modifica
    } a taxa de ganho de imunidade em {NATURALFIXED($gainrate, 5)}, a força em {NATURALFIXED($strength, 5)} por pelo menos {NATURALFIXED($time, 3)} {MANY("second", $time)}

reagent-effect-guidebook-disease-progress-change =
    { $chance ->
        [1] Modifica
        *[other] modifica
    } o progresso de doenças do tipo {$type} em {NATURALFIXED($amount, 5)}

reagent-effect-guidebook-disease-mutate = Muta doenças em {NATURALFIXED($amount, 4)}

