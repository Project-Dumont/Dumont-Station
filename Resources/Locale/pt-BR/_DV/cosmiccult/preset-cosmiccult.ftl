## RODADA DO CULTO CÓSMICO, ANTAG & TEXTO DO MODO DE JOGO

cosmiccult-announcement-sender = ???

cosmiccult-title = Culto Cósmico
cosmiccult-description = Cultistas espreitam entre a tripulação.

roles-antag-cosmiccult-name = Cultista Cósmico
roles-antag-cosmiccult-description = Traga o fim de todas as coisas através de subterfúgio e sabotagem, doutrinando aqueles que se opuserem a você.

cosmiccult-gamemode-title = O Culto Cósmico
cosmiccult-gamemode-description = Scanners detectam um aumento anômalo em Λ-CDM. Não há dados adicionais.

cosmiccult-vote-steward-initiator = O Desconhecido

cosmiccult-vote-steward-title = Mordomia do Culto Cósmico
cosmiccult-vote-steward-briefing =
    Você é o Mordomo do Culto Cósmico!
    Certifique-se de que O Monólito seja colocado em um local seguro e organize o culto para garantir a vitória coletiva.
    Você não tem permissão para instruir cultistas sobre como usar ou gastar sua Entropia.

cosmiccult-vote-lone-steward-title = O Cultista Solitário
cosmiccult-vote-lone-steward-briefing =
    Você está completamente sozinho. Mas seu dever não acabou.
    Certifique-se de que O Monólito seja colocado em um local seguro e termine o que o culto começou.

cosmiccult-finale-autocall-briefing = O Monólito será ativado em {$minutesandseconds}! Reúnam-se e preparem-se para o fim.
cosmiccult-finale-ready = Uma luz aterrorizante irrompe do Monólito!
cosmiccult-finale-speedup = O chamado acelera! Energia pulsa pelo ambiente...

cosmiccult-finale-degen = Você sente-se se desfazendo!
cosmiccult-finale-location = Scanners detectam um enorme pico de Λ-CDM em {$location}!
cosmiccult-finale-cancel-begin = A força de vontade de sua mente começa a quebrar o ritual...
cosmiccult-finale-beckon-begin = Os sussurros no fundo da sua mente se intensificam...
cosmiccult-finale-beckon-success = Você convoca o ato final.

cosmiccult-monument-powerdown = O Monólito fica estranhamente silencioso.

## TEXTO DE FINAL DE RODADA

cosmiccult-roundend-cultist-count = {$initialCount ->
    [1] Havia {$initialCount} [color=#4cabb3]Cultista Cósmico[/color].
    *[other] Havia {$initialCount} [color=#4cabb3]Cultistas Cósmicos[/color].
}
cosmiccult-roundend-entropy-count = O culto drenou {$count} Entropia.
cosmiccult-roundend-cultpop-count = Cultistas compunham {$count}% da tripulação.
cosmiccult-roundend-monument-stage = {$stage ->
    [1] Infelizmente, o Monólito parece abandonado.
    [2] O Monólito progrediu, mas a conclusão estava fora de alcance.
    [3] O Monólito foi concluído.
    *[other] [color=red]Algo deu MUITO errado.[/color]
}

cosmiccult-roundend-cultcomplete = [color=#4cabb3]Vitória completa do Culto Cósmico![/color]
cosmiccult-roundend-cultmajor = [color=#4cabb3]Grande vitória do Culto Cósmico![/color]
cosmiccult-roundend-cultminor = [color=#4cabb3]Vitória menor do Culto Cósmico![/color]
cosmiccult-roundend-neutral = [color=yellow]Final neutro![/color]
cosmiccult-roundend-crewminor = [color=green]Vitória menor da tripulação![/color]
cosmiccult-roundend-crewmajor = [color=green]Grande vitória da tripulação![/color]
cosmiccult-roundend-crewcomplete = [color=green]Vitória completa da tripulação![/color]

cosmiccult-summary-cultcomplete = Os cultistas cósmicos trouxeram o fim!
cosmiccult-summary-cultmajor = A vitória dos cultistas cósmicos será inevitável.
cosmiccult-summary-cultminor = O Monólito foi concluído, mas não totalmente energizado.
cosmiccult-summary-neutral = O culto viverá para ver outro dia.
cosmiccult-summary-crewminor = O culto ficou sem mordomo.
cosmiccult-summary-crewmajor = Todos os cultistas cósmicos foram eliminados.
cosmiccult-summary-crewcomplete = Cada cultista cósmico foi deconvertido!

cosmiccult-elimination-shuttle-call = Com base em varreduras de nossos sensores de longo alcance, a anomalia Λ-CDM diminuiu. Agradecemos sua prudência. Uma nave de emergência foi automaticamente chamada para a estação para procedimentos de descontaminação e debriefing. ETA: {$time} {$units}. Observe que, se o impacto psicológico da anomalia for insignificante, você pode chamar de volta a nave para estender o turno.
cosmiccult-elimination-announcement = Com base em varreduras de nossos sensores de longo alcance, a anomalia Λ-CDM diminuiu. Agradecemos sua prudência. Uma nave de emergência já está a caminho. Retorne em segurança para a CentComm para procedimentos de descontaminação e debriefing.

## INSTRUÇÕES

cosmiccult-role-roundstart-fluff =
    Ao se preparar para mais um turno em mais uma estação NanoTrasen, um conhecimento indescritível de repente invade sua mente!
    Uma revelação sem igual. Um fim para o sofrimento cíclico e sisifiano.
    Uma cortina suave se fecha.

    Tudo o que você precisa fazer é trazê-la.

cosmiccult-role-short-briefing =
    Você é um Cultista Cósmico!
    Seus objetivos estão listados no menu de personagem.
    Leia mais sobre seu papel na entrada do guia.

cosmiccult-role-conversion-fluff =
    Ao completar a invocação, um conhecimento indescritível de repente invade sua mente!
    Uma revelação sem igual. Um fim para o sofrimento cíclico e sisifiano.
    Uma cortina suave se fecha.

    Tudo o que você precisa fazer é trazê-la.

cosmiccult-role-deconverted-fluff =
    Um grande vazio invade sua mente. Um vazio reconfortante, porém desconhecido...
    Todos os pensamentos e memórias do seu tempo no culto começam a desaparecer e se embaralhar.

cosmiccult-role-deconverted-briefing =
    Deconvertido!
    Você não é mais um Cultista Cósmico.

cosmiccult-monument-stage1-briefing =
    O Monólito foi convocado.
    Ele está localizado em {$location}!

cosmiccult-monument-stage2-briefing =
    O Monólito cresce em poder!
    Sua influência afetará o espaço real em {$time} {MANY("segundo",$time)}.

cosmiccult-monument-stage3-briefing =
    O Monólito foi concluído!
    Sua influência começará a se sobrepor ao espaço real em {$time} {MANY("segundo",$time)}.
    Este é o trecho final! Reúna o máximo de Entropia que puder.

## FENDAS MALIGNAS

cosmiccult-rift-inuse = Você não pode fazer isso agora.
cosmiccult-rift-invaliduser = Você não tem as ferramentas adequadas para lidar com isso.
cosmiccult-rift-chaplainoops = Empunhe sua escritura sagrada.
cosmiccult-rift-alreadyempowered = Você já está energizado; o poder da fenda seria desperdiçado.
cosmiccult-rift-beginabsorb = A fenda começa a se fundir com você...
cosmiccult-rift-beginpurge = Sua consagração começa a purificar a fenda maligna...

cosmiccult-rift-absorb = {CAPITALIZE(ARTIGO-O($NAME))} {$NAME} absorve a fenda, e uma luz maligna energiza seu corpo!
cosmiccult-rift-purge = {CAPITALIZE(ARTIGO-O($NAME))} {$NAME} purifica a fenda maligna da realidade!

## UI / POPUP BASE

cosmiccult-ui-deconverted-title = Deconvertido
cosmiccult-ui-converted-title = Convertido
cosmiccult-ui-roundstart-title = O Desconhecido

cosmiccult-ui-converted-text-1 =
    Você foi convertido em um Cultista Cósmico.
cosmiccult-ui-converted-text-2 =
    Ajude o culto em seus objetivos enquanto garante seu segredo.
    Coopere com os planos de seus colegas cultistas.

cosmiccult-ui-roundstart-text-1 =
    Você é um Cultista Cósmico!
cosmiccult-ui-roundstart-text-2 =
    Ajude o culto em seus objetivos enquanto garante seu segredo.
    Siga as orientações do seu mordomo do culto.

cosmiccult-ui-deconverted-text-1 =
    Você não é mais um Cultista Cósmico.
cosmiccult-ui-deconverted-text-2 =
    Você perdeu todas as memórias relacionadas ao Culto Cósmico.
    Se for convertido novamente, essas memórias retornarão.

cosmiccult-ui-popup-confirm = Confirmar

## OBJETIVOS / MENU DE PERSONAGEM

objective-issuer-cosmiccult = [bold][color=#cae8e8]O Desconhecido[/color][/bold]

objective-cosmiccult-charactermenu = Você deve trazer o fim de todas as coisas. Complete suas tarefas para avançar o progresso do culto.
objective-cosmiccult-steward-charactermenu = Você deve liderar o culto para trazer o fim de todas as coisas. Supervisionar e garantir o progresso do culto.

objective-condition-entropy-title = DRENAR ENTROPIA
objective-condition-entropy-desc = Drene coletivamente pelo menos {$count} {MANY("Entropia", $count)} da tripulação.
objective-condition-culttier-title = POTENCIALIZAR O MONÓLITO
objective-condition-culttier-desc = Certifique-se de que O Monólito seja levado ao poder máximo.
objective-condition-victory-title = TRAGA O FIM
objective-condition-victory-desc = Convoque O Desconhecido e anuncie o ato final.

## ANÚNCIOS DE CHAT

cosmiccult-radio-tier1-progress = O Monólito foi convocado para a estação...

cosmiccult-announce-tier2-progress = Uma dormência inquietante arrepia seus sentidos.
cosmiccult-announce-tier2-warning = Scanners detectam um aumento notável em Λ-CDM! Fendas no espaço real podem aparecer em breve. Por favor, avise o capelão da estação se avistar alguma.

cosmiccult-announce-tier3-progress = Arcos de energia noosférica crepitam pela estrutura da estação. O fim se aproxima.
cosmiccult-announce-tier3-warning = Aumento crítico em Λ-CDM detectado. Pessoal infectado deve ser contido ou neutralizado imediatamente.

cosmiccult-announce-finale-warning = Toda a tripulação da estação. A anomalia Λ-CDM está indo para o supercrítico, instrumentos falhando; horizonte de evento de transição noosférica-para-real IMINENTE. Se você ainda não está em protocolo de contenção, intervenha imediatamente. Repito: Intervenha imediatamente ou morra.

cosmiccult-announce-victory-summon = UMA FRAÇÃO DO PODER CÓSMICO É INVOCADA.


## DIVERSOS

cosmiccult-spire-entropy = Um fragmento de Entropia se condensa na superfície da torre.
cosmiccult-entropy-inserted = Você infunde {$count} {MANY("Entropia", $count)} no Monólito.
cosmiccult-entropy-unavailable = Você não pode fazer isso agora.
cosmiccult-astral-ascendant = {$name}, Ascendente
cosmiccult-gear-pickup-rejection = {CAPITALIZE(ARTIGO-O($ITEM))} {$ITEM} resiste ao toque {PREPOSICAO-DE($TARGET)} {$TARGET}!
cosmiccult-gear-pickup = Você se sente se desfazendo ao segurar {ARTIGO-O($ITEM)} {$ITEM}!

# Goobstation

cult-alert-recall-shuttle = Altas concentrações de Λ-CDM de origem desconhecida detectadas a bordo da estação. Todas as presenças anômalas devem ser eliminadas ou contidas para que a evacuação possa ser autorizada.
