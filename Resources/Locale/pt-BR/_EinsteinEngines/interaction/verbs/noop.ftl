interaction-LookAt-name = Encarar
interaction-LookAt-description = Encare o vazio e veja-o encarar de volta.
interaction-LookAt-success-self-popup = Você encara { ARTIGO-O($user) } { $user }.
interaction-LookAt-success-target-popup = Você sente { ARTIGO-O($user) } { $user } te encarando...
interaction-LookAt-success-others-popup = { CAPITALIZE(ARTIGO-O($user)) } { $user } encara { ARTIGO-O($target) } { $target }.

interaction-Hug-name = Abraçar
interaction-Hug-description = Um abraço por dia afasta os horrores psicológicos além da sua compreensão.
interaction-Hug-success-self-popup = Você abraça { ARTIGO-O($target) } { $target }.
interaction-Hug-success-target-popup = { CAPITALIZE(ARTIGO-O($user)) } { $user } te abraça.
interaction-Hug-success-others-popup = { CAPITALIZE(ARTIGO-O($user)) } { $user } abraça { ARTIGO-O($target) } { $target }.

interaction-Pet-name = Fazer carinho
interaction-Pet-description = Faça carinho no seu colega de trabalho para aliviar o estresse dele.
interaction-Pet-success-self-popup = Você faz carinho { PREPOSICAO-EM($target) } { $target } na cabeça { PRONOME-DELE($target) }.
interaction-Pet-success-target-popup = { CAPITALIZE(ARTIGO-O($user)) } { $user } faz carinho em você na sua cabeça.
interaction-Pet-success-others-popup = { CAPITALIZE(ARTIGO-O($user)) } { $user } faz carinho { PREPOSICAO-EM($target) } { $target }.

interaction-KnockOn-name = Bater
interaction-KnockOn-description = Bata no alvo para chamar atenção.
interaction-KnockOn-success-self-popup = Você bate { PREPOSICAO-EM($target) } { $target }.
interaction-KnockOn-success-target-popup = { CAPITALIZE(ARTIGO-O($user)) } { $user } bate em você.
interaction-KnockOn-success-others-popup = { CAPITALIZE(ARTIGO-O($user)) } { $user } bate { PREPOSICAO-EM($target) } { $target }.

interaction-Rattle-name = Chacoalhar
interaction-Rattle-success-self-popup = Você chacoalha { ARTIGO-O($target) } { $target }.
interaction-Rattle-success-target-popup = { CAPITALIZE(ARTIGO-O($user)) } { $user } te chacoalha.
interaction-Rattle-success-others-popup = { CAPITALIZE(ARTIGO-O($user)) } { $user } chacoalha { ARTIGO-O($target) } { $target }.

# O abaixo inclui condicionais para se o usuário está segurando um item
interaction-WaveAt-name = Acenar para
interaction-WaveAt-description = Acene para o alvo. Se estiver segurando um item, você irá acenar com ele.
interaction-WaveAt-success-self-popup = Você acena {$hasUsed ->
    [false] para { ARTIGO-O($target) } { $target }.
    *[true] com { PRONOME-SEU($used) } { $used } para { ARTIGO-O($target) } { $target }.
}
interaction-WaveAt-success-target-popup = { CAPITALIZE(ARTIGO-O($user)) } { $user } acena {$hasUsed ->
    [false] para você.
    *[true] com { ARTIGO-O($used) } { $used } { PRONOME-DELE($user) } para você.
}
interaction-WaveAt-success-others-popup = { CAPITALIZE(ARTIGO-O($user)) } { $user } acena {$hasUsed ->
    [false] para { ARTIGO-O($target) } { $target }.
    *[true] com { ARTIGO-O($used) } { $used } { PRONOME-DELE($user) } para { ARTIGO-O($target) } { $target }.
}
