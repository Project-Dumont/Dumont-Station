interaction-LookAt-name = Stare
interaction-LookAt-description = Stare into the void and see it stare back.
interaction-LookAt-success-self-popup = You stare at {THE($target)}.
interaction-LookAt-success-target-popup = You feel {THE($user)} staring at you...
interaction-LookAt-success-others-popup = {THE($user)} stares at {THE($target)}.

interaction-Hug-name = Hug
interaction-Hug-description = A hug a day keeps the psychological horrors beyond your comprehension away.
interaction-Hug-success-self-popup = You hug {THE($target)}.
interaction-Hug-success-target-popup = {THE($user)} hugs you.
interaction-Hug-success-others-popup = {THE($user)} hugs {THE($target)}.

interaction-KnockOn-name = Knock
interaction-KnockOn-description = Knock on the target to attract attention.
interaction-KnockOn-success-self-popup = You knock on {THE($target)}.
interaction-KnockOn-success-target-popup = {THE($user)} knocks on you.
interaction-KnockOn-success-others-popup = {THE($user)} knocks on {THE($target)}.

# The below includes conditionals for if the user is holding an item
interaction-WaveAt-name = Wave at
interaction-WaveAt-description = Wave at the target. If you are holding an item, you will wave it.
interaction-WaveAt-success-self-popup = You wave {$hasUsed ->
    [false] at {THE($target)}.
    *[true] your {$used} at {THE($target)}.
}
interaction-WaveAt-success-target-popup = {THE($user)} waves {$hasUsed ->
    [false] at you.
    *[true] {POSS-PRONOUN($user)} {$used} at you.
}
interaction-WaveAt-success-others-popup = {THE($user)} waves {$hasUsed ->
    [false] at {THE($target)}.
    *[true] {POSS-PRONOUN($user)} {$used} at {THE($target)}.interaction-LookAt-name = Encostar o olhar
interaction-LookAt-description = Encare o vazio e sinta ele encarar você de volta.
interaction-LookAt-success-self-popup = Você encara {THE($target)}.
interaction-LookAt-success-target-popup = Você sente {THE($user)} te encarando...
interaction-LookAt-success-others-popup = {THE($user)} encara {THE($target)}.

interaction-Hug-name = Abraçar
interaction-Hug-description = Um abraço por dia mantém os horrores psicológicos além da sua compreensão longe.
interaction-Hug-success-self-popup = Você abraça {THE($target)}.
interaction-Hug-success-target-popup = {THE($user)} te abraça.
interaction-Hug-success-others-popup = {THE($user)} abraça {THE($target)}.

interaction-KnockOn-name = Bater
interaction-KnockOn-description = Bata no alvo para chamar atenção.
interaction-KnockOn-success-self-popup = Você bate em {THE($target)}.
interaction-KnockOn-success-target-popup = {THE($user)} bate em você.
interaction-KnockOn-success-others-popup = {THE($user)} bate em {THE($target)}.

interaction-WaveAt-name = Acenar
interaction-WaveAt-description = Acene para o alvo. Se você estiver segurando um item, você vai acenar com ele.
interaction-WaveAt-success-self-popup = Você acena {$hasUsed ->
    [false] para {THE($target)}.
    *[true] com seu {$used} para {THE($target)}.
}
interaction-WaveAt-success-target-popup = {THE($user)} acena {$hasUsed ->
    [false] para você.
    *[true] com {POSS-PRONOUN($user)} {$used} para você.
}
interaction-WaveAt-success-others-popup = {THE($user)} acena {$hasUsed ->
    [false] para {THE($target)}.
    *[true] com {POSS-PRONOUN($user)} {$used} para {THE($target)}.
}

