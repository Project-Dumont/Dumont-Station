entity-effect-guidebook-modify-disgust =
    { $chance ->
        [1] { $deltasign ->
                [1] Aumenta
                *[-1] Diminui
            }
        *[other]
            { $deltasign ->
                [1] aumenta
                *[-1] diminui
            }
    } o nível de nojo em { $amount }
