# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

reagent-effect-condition-guidebook-stamina-damage-threshold =
    { $max ->
        [2147483648] o alvo tem pelo menos {NATURALFIXED($min, 2)} de dano de stamina
        *[other] { $min ->
                    [0] o alvo tem no máximo {NATURALFIXED($max, 2)} de dano de stamina
                    *[other] o alvo tem entre {NATURALFIXED($min, 2)} e {NATURALFIXED($max, 2)} de dano de stamina
                 }
    }

reagent-effect-condition-guidebook-unique-bloodstream-chem-threshold =
    { $max ->
        [2147483648] { $min ->
                        [1] há pelo menos {$min} reagente
                        *[other] há pelo menos {$min} reagentes
                     }
        [1] { $min ->
               [0] há no máximo {$max} reagente
               *[other] há entre {$min} e {$max} reagentes
            }
        *[other] { $min ->
                    [-1] há no máximo {$max} reagentes
                    *[other] há entre {$min} e {$max} reagentes
                 }
    }

reagent-effect-condition-guidebook-typed-damage-threshold =
    { $inverse ->
        [true] o alvo tem no máximo
        *[false] o alvo tem pelo menos
    } { $changes } de dano
