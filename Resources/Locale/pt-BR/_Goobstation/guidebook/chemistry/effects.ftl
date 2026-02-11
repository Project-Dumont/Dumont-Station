reagent-effect-guidebook-deal-stamina-damage =
    { $chance ->
        [1] { $deltasign ->
                [1] Fere
                *[-1] Cura
            }
        *[other]
            { $deltasign ->
                [1] fere
                *[-1] cura
            }
    } { $amount } { $immediate ->
                    [true] imediatamente
                    *[false] ao longo do tempo
                  } de dano de estamina

reagent-effect-guidebook-immunity-modifier =
    { $chance ->
        [1] Modifica
        *[other] modifica
    } o ganho de imunidade por {NATURALFIXED($gainrate, 5)}, fortalece por {NATURALFIXED($strength, 5)} por pelo menos {NATURALFIXED($time, 3)} {MANY("segundo", $time)}

reagent-effect-guidebook-disease-progress-change =
    { $chance ->
        [1] Modifica
        *[other] modifica
    } o progresso da doença {$type} por {NATURALFIXED($amount, 5)}
