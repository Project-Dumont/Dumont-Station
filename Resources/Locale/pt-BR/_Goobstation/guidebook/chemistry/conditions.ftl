reagent-effect-condition-guidebook-stamina-damage-threshold =
    { $max ->
        [2147483648] o alvo tem pelo menos { NATURALFIXED($min, 2) } de dano de estamina
        *[other] { $min ->
                    [0] o alvo tem no máximo { NATURALFIXED($max, 2) } de dano de estamina
                    *[other] o alvo tem entre { NATURALFIXED($min, 2) } e { NATURALFIXED($max, 2) } de dano de estamina
                 }
    }

reagent-effect-condition-guidebook-unique-bloodstream-chem-threshold =
    { $max ->
        [2147483648] { $min ->
                        [1] tem pelo menos {$min} reagente
                        *[other] tem pelo menos {$min} reagentes
                     }
        [1] { $min ->
               [0] tem no máximo {$max} reagente
               *[other] tem entre {$min} e {$max} reagentes
            }
        *[other] { $min ->
                    [-1] tem no máximo {$max} reagentes
                    *[other] tem entre {$min} e {$max} reagentes
                 }
    }

reagent-effect-condition-guidebook-typed-damage-threshold =
    { $inverse ->
        [true] o alvo tem no máximo
        *[false] o alvo tem no mínimo
    } { $changes } de dano
