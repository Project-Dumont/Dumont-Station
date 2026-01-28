## COSMIC CULT ROUND, ANTAG & GAMEMODE TEXT

cosmiccult-announcement-sender = ???

cosmiccult-title = Culto Cósmico
cosmiccult-description = Cultistas se escondem entre a tripulação.

roles-antag-cosmiccult-name = Cultista Cósmico
roles-antag-cosmiccult-description = Conduza o fim de todas as coisas através de subterfúgio e sabotagem, doutrinando aqueles que se opõem a você.

cosmiccult-gamemode-title = O Culto Cósmico
cosmiccult-gamemode-description = Escâneres detectam um aumento anômalo em Λ-CDM. Não há dados adicionais.

cosmiccult-vote-steward-initiator = O Desconhecido

cosmiccult-vote-steward-title = Tutela do Culto Cósmico
cosmiccult-vote-steward-briefing =
    Você é o Tutor do Culto Cósmico!
    Garanta que O Monumento seja colocado em um local seguro e organize o culto para assegurar a vitória coletiva.
    Você não tem permissão para instruir os cultistas sobre como usar ou gastar sua Entropia.

cosmiccult-vote-lone-steward-title = O Cultista Solitário
cosmiccult-vote-lone-steward-briefing =
    Você está completamente sozinho. Mas seu dever ainda não terminou.
    Garanta que O Monumento seja colocado em um local seguro e conclua o que o culto começou.

cosmiccult-finale-autocall-briefing = O Monumento será ativado em {$minutesandseconds}! Reúnam-se e preparem-se para o fim.
cosmiccult-finale-ready = Uma luz aterradora irrompe do Monumento!
cosmiccult-finale-speedup = O chamado se intensifica! Energia percorre os arredores...

cosmiccult-finale-degen = Você sente seu ser se desfazer!
cosmiccult-finale-location = Escâneres detectam um enorme pico de Λ-CDM {$location}!
cosmiccult-finale-cancel-begin = A força de vontade da sua mente começa a despedaçar o ritual...
cosmiccult-finale-beckon-begin = Os sussurros no fundo da sua mente se intensificam...
cosmiccult-finale-beckon-success = Você convoca o chamado final do pano.

cosmiccult-monument-powerdown = O Monumento cai em um silêncio inquietante.

## ROUNDEND TEXT

cosmiccult-roundend-cultist-count = {$initialCount ->
    [1] Havia {$initialCount} [color=#4cabb3]Cultista Cósmico[/color].
    *[other] Havia {$initialCount} [color=#4cabb3]Cultistas Cósmicos[/color].
}
cosmiccult-roundend-entropy-count = O culto drenou {$count} Entropia.
cosmiccult-roundend-cultpop-count = Cultistas compunham {$count}% da tripulação.
cosmiccult-roundend-monument-stage = {$stage ->
    [1] Alas, o Monumento parece abandonado.
    [2] O Monumento progrediu, mas a conclusão estava fora de alcance.
    [3] O Monumento foi concluído.
    *[other] [color=red]Algo deu MUITO errado.[/color]
}

cosmiccult-roundend-cultcomplete = [color=#4cabb3]Vitória completa do Culto Cósmico![/color]
cosmiccult-roundend-cultmajor = [color=#4cabb3]Vitória maior do Culto Cósmico![/color]
cosmiccult-roundend-cultminor = [color=#4cabb3]Vitória menor do Culto Cósmico![/color]
cosmiccult-roundend-neutral = [color=yellow]Final neutro![/color]
cosmiccult-roundend-crewminor = [color=green]Vitória menor da tripulação![/color]
cosmiccult-roundend-crewmajor = [color=green]Vitória maior da tripulação![/color]
cosmiccult-roundend-crewcomplete = [color=green]Vitória completa da tripulação![/color]

cosmiccult-summary-cultcomplete = Os cultistas cósmicos conduziram o fim!
cosmiccult-summary-cultmajor = A vitória dos cultistas cósmicos será inevitável.
cosmiccult-summary-cultminor = O Monumento foi concluído, mas não totalmente fortalecido.
cosmiccult-summary-neutral = O culto viverá para ver outro dia.
cosmiccult-summary-crewminor = O culto ficou sem tutor.
cosmiccult-summary-crewmajor = Todos os cultistas cósmicos foram eliminados.
cosmiccult-summary-crewcomplete = Todos os cultistas cósmicos foram desconvertidos!

cosmiccult-elimination-shuttle-call = Com base em leituras de nossos sensores de longo alcance, a anomalia Λ-CDM diminuiu. Agradecemos sua prudência. Um ônibus de emergência foi automaticamente chamado à estação para procedimentos de descontaminação e debriefing. ETA: {$time} {$units}.
cosmiccult-elimination-announcement = Com base em leituras de nossos sensores de longo alcance, a anomalia Λ-CDM diminuiu. Agradecemos sua prudência. Um ônibus de emergência já está a caminho. Retornem ao CentComm com segurança para procedimentos de descontaminação e debriefing.

## BRIEFINGS

cosmiccult-role-roundstart-fluff =
    Enquanto você se prepara para mais um turno a bordo de mais uma estação da NanoTrasen, um conhecimento incontável invade sua mente!
    Uma revelação sem igual. Um fim para o sofrimento cíclico e sisifiano.
    Um suave chamado do pano.

    Tudo o que você precisa fazer é conduzi-lo.

cosmiccult-role-short-briefing =
    Você é um Cultista Cósmico!
    Seus objetivos estão listados no menu do personagem.
    Leia mais sobre seu papel na entrada do guia.

cosmiccult-role-conversion-fluff =
    Quando a invocação se completa, um conhecimento incontável invade sua mente!
    Uma revelação sem igual. Um fim para o sofrimento cíclico e sisifiano.
    Um suave chamado do pano.

    Tudo o que você precisa fazer é conduzi-lo.

cosmiccult-role-deconverted-fluff =
    Um grande vazio atravessa sua mente. Um vazio reconfortante, porém desconhecido...
    Todos os pensamentos e memórias do seu tempo no culto começam a desaparecer e se embaralhar.

cosmiccult-role-deconverted-briefing =
    Desconvertido!
    Você não é mais um Cultista Cósmico.

cosmiccult-monument-stage1-briefing =
    O Monumento foi convocado.
    Ele está localizado em {$location}!

cosmiccult-monument-stage2-briefing =
    O Monumento cresce em poder!
    Sua influência afetará o espaço real em {$time} segundos.

cosmiccult-monument-stage3-briefing =
    O Monumento foi concluído!
    Sua influência começará a se sobrepor ao espaço real em {$time} segundos.
    Este é o trecho final! Reúna o máximo de entropia que conseguir.

## MALIGN RIFTS

cosmiccult-rift-inuse = Você não pode fazer isso agora.
cosmiccult-rift-invaliduser = Você não possui as ferramentas adequadas para lidar com isso.
cosmiccult-rift-chaplainoops = Empunhe sua escritura sagrada.
cosmiccult-rift-alreadyempowered = Você já está fortalecido; o poder da fenda seria desperdiçado.
cosmiccult-rift-beginabsorb = A fenda começa a se fundir com você...
cosmiccult-rift-beginpurge = Sua consagração começa a purgar a fenda maligna...

cosmiccult-rift-absorb = {$NAME} absorve a fenda, e uma luz maligna fortalece seu corpo!
cosmiccult-rift-purge = {$NAME} purga a fenda maligna da realidade!

## UI / BASE POPUP

cosmiccult-ui-deconverted-title = Desconvertido
cosmiccult-ui-converted-title = Convertido
cosmiccult-ui-roundstart-title = O Desconhecido

cosmiccult-ui-converted-text-1 =
    Você foi convertido em um Cultista Cósmico.
cosmiccult-ui-converted-text-2 =
    Ajude o culto em seus objetivos enquanto garante seu sigilo.
    Coopere com os planos de seus companheiros cultistas.

cosmiccult-ui-roundstart-text-1 =
    Você é um Cultista Cósmico!
cosmiccult-ui-roundstart-text-2 =
    Ajude o culto em seus objetivos enquanto garante seu sigilo.
    Siga as instruções do tutor do culto.

cosmiccult-ui-deconverted-text-1 =
    Você não é mais um Cultista Cósmico.
cosmiccult-ui-deconverted-text-2 =
    Você perdeu todas as memórias relacionadas ao Culto Cósmico.
    Se for convertido novamente, essas memórias retornarão.

cosmiccult-ui-popup-confirm = Confirmar

## OBJECTIVES / CHARACTERMENU

objective-issuer-cosmiccult = [bold][color=#cae8e8]O Desconhecido[/color][/bold]

objective-cosmiccult-charactermenu = Você deve conduzir o fim de todas as coisas. Complete suas tarefas para avançar o progresso do culto.
objective-cosmiccult-steward-charactermenu = Você deve guiar o culto para conduzir o fim de todas as coisas. Supervise e garanta o progresso do culto.

objective-condition-entropy-title = DRENAR ENTROPIA
objective-condition-entropy-desc = Drene coletivamente pelo menos {$count} de entropia da tripulação.
objective-condition-culttier-title = FORTALECER O MONUMENTO
objective-condition-culttier-desc = Garanta que O Monumento seja levado ao poder máximo.
objective-condition-victory-title = CONDUZIR O FIM
objective-condition-victory-desc = Convocar O Desconhecido e anunciar o chamado final do pano.

## CHAT ANNOUNCEMENTS

cosmiccult-radio-tier1-progress = O Monumento é convocado à estação...

cosmiccult-announce-tier2-progress = Uma dormência inquietante arrepia seus sentidos.
cosmiccult-announce-tier2-warning = Escâneres detectam um aumento notável em Λ-CDM! Fendas no espaço real podem aparecer em breve. Por favor, alerte o capelão da estação se avistadas.

cosmiccult-announce-tier3-progress = Arcos de energia noosférica crepitam pela estrutura gemedora da estação. O fim se aproxima.
cosmiccult-announce-tier3-warning = Aumento crítico de Λ-CDM detectado. Pessoal infectado deve ser subjugado ou neutralizado à vista.

cosmiccult-announce-finale-warning = Atenção, tripulação da estação. A anomalia Λ-CDM está se tornando supercrítica, instrumentos falhando; horizonte de eventos de transição noosférica-para-real IMINENTE. Se você não estiver em contra-protocolo, mobilize-se imediatamente e intervenha. Repito: intervenha imediatamente ou morra.

cosmiccult-announce-victory-summon = UMA FRAÇÃO DO PODER CÓSMICO É CONVOCADA.

## MISC

cosmiccult-spire-entropy = Um mote de entropia se condensa na superfície da espira.
cosmiccult-entropy-inserted = Você infunde {$count} de entropia no Monumento.
cosmiccult-entropy-unavailable = Você não pode fazer isso agora.
cosmiccult-astral-ascendant = {$name}, Ascendente
cosmiccult-gear-pickup-rejection = O {$ITEM} resiste ao toque de {CAPITALIZE(THE($TARGET))}!
cosmiccult-gear-pickup = Você sente seu ser se desfazer enquanto segura o {$ITEM}!

# Goobstation

cult-alert-recall-shuttle = Altas concentrações de Λ-CDM de origem desconhecida detectadas a bordo da estação. Todas as presenças anômalas devem ser purgadas ou contidas antes que a evacuação possa ser autorizada.
