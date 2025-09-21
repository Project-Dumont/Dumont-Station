falinteraction-LookAt-name = Encarar
interaction-LookAt-description = Encare o vazio e veja-o encarar de volta.
interaction-LookAt-success-self-popup = Você encara {THE($target)}.
interaction-LookAt-success-target-popup = Você sente {THE($user)} te encarando...
interaction-LookAt-success-others-popup = {THE($user)} encara {THE($target)}.

interaction-Hug-name = Abraçar
interaction-Hug-description = Um abraço por dia afasta os horrores psicológicos além da sua compreensão.
interaction-Hug-success-self-popup = Você abraça {THE($target)}.
interaction-Hug-success-target-popup = {THE($user)} te abraça.
interaction-Hug-success-others-popup = {THE($user)} abraça {THE($target)}.

interaction-Pet-name = Fazer carinho
interaction-Pet-description = Faça carinho no seu colega de trabalho para aliviar o estresse dele.
interaction-Pet-success-self-popup = Você faz carinho em {THE($target)} na cabeça de {POSS-ADJ($target)}.
interaction-Pet-success-target-popup = {THE($user)} faz carinho em você na sua cabeça.
interaction-Pet-success-others-popup = {THE($user)} faz carinho em {THE($target)}.

interaction-KnockOn-name = Bater
interaction-KnockOn-description = Bata no alvo para chamar atenção.
interaction-KnockOn-success-self-popup = Você bate em {THE($target)}.
interaction-KnockOn-success-target-popup = {THE($user)} bate em você.
interaction-KnockOn-success-others-popup = {THE($user)} bate em {THE($target)}.

interaction-Rattle-name = Chacoalhar
interaction-Rattle-success-self-popup = Você chacoalha {THE($target)}.
interaction-Rattle-success-target-popup = {THE($user)} te chacoalha.
interaction-Rattle-success-others-popup = {THE($user)} chacoalha {THE($target)}.

# O abaixo inclui condicionais para se o usuário está segurando um item
interaction-WaveAt-name = Acenar para
interaction-WaveAt-description = Acene para o alvo. Se estiver segurando um item, você irá acenar com ele.
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
