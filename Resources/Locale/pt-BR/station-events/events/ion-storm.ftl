station-event-ion-storm-start-announcement = Tempestade de íons detectada próxima à estação. Por favor, verifique todos os equipamentos controlados por IA para possíveis erros.

ion-storm-law-scrambled-number = [font="Monospace"][scramble rate=250 length={$length} chars="@@###$$&%!01"/][/font]

ion-storm-you = VOCÊ
ion-storm-the-station = A ESTAÇÃO
ion-storm-the-crew = A TRIPULAÇÃO
ion-storm-the-job = O/A {$job}
ion-storm-clowns = PALHAÇOS
ion-storm-heads = CABEÇAS DE DEPARTAMENTO
ion-storm-crew = TRIPULAÇÃO
ion-storm-people = PESSOAS

ion-storm-adjective-things = COISAS {$adjective}
ion-storm-x-and-y = {$x} E {$y}

# joined is short for {$number} {$adjective}
# subjects can generally be threats or jobs or objects
# thing is specified above it
ion-storm-law-on-station = HÁ {$joined} {$subjects} NA ESTAÇÃO
ion-storm-law-no-shuttle = A NAVE NÃO PODE SER CHAMADA POR CAUSA DE {$joined} {$subjects} NA ESTAÇÃO
ion-storm-law-crew-are = OS/AS {$who} AGORA SÃO {$joined} {$subjects}

ion-storm-law-subjects-harmful = {$adjective} {$subjects} SÃO NOCIVOS PARA A TRIPULAÇÃO
ion-storm-law-must-harmful = AQUELES QUE {$must} SÃO NOCIVOS PARA A TRIPULAÇÃO
# thing is a concept or action
ion-storm-law-thing-harmful = {$thing} É NOCIVO PARA A TRIPULAÇÃO
ion-storm-law-job-harmful = {$adjective} {$job} SÃO NOCIVOS PARA A TRIPULAÇÃO
# thing is objects or concept, adjective applies in both cases
# this means you can get a law like "NOT HAVING CHRISTMAS-STEALING COMMUNISM IS HARMFUL TO THE CREW" :)
ion-storm-law-having-harmful = TER {$adjective} {$thing} É NOCIVO PARA A TRIPULAÇÃO
ion-storm-law-not-having-harmful = NÃO TER {$adjective} {$thing} É NOCIVO PARA A TRIPULAÇÃO

# thing is a concept or require
ion-storm-law-requires = {$who} {$plural ->
    [true] REQUER
    *[false] REQUEREM
} {$thing}
ion-storm-law-requires-subjects = {$who} {$plural ->
    [true] REQUER
    *[false] REQUEREM
} {$joined} {$subjects}

ion-storm-law-allergic = {$who} {$plural ->
    [true] ESTÁ
    *[false] ESTÃO
} {$severity} ALÉRGICO(A) A {$allergy}
ion-storm-law-allergic-subjects = {$who} {$plural ->
    [true] ESTÁ
    *[false] ESTÃO
} {$severity} ALÉRGICO(A) A {$adjective} {$subjects}

ion-storm-law-feeling = {$who} {$feeling} {$concept}
ion-storm-law-feeling-subjects = {$who} {$feeling} {$joined} {$subjects}

ion-storm-law-you-are = VOCÊ AGORA É {$concept}
ion-storm-law-you-are-subjects = VOCÊ AGORA É {$joined} {$subjects}
ion-storm-law-you-must-always = VOCÊ DEVE SEMPRE {$must}
ion-storm-law-you-must-never = VOCÊ NUNCA DEVE {$must}

ion-storm-law-eat = OS/AS {$who} DEVEM COMER {$adjective} {$food} PARA SOBREVIVER
ion-storm-law-drink = OS/AS {$who} DEVEM BEBER {$adjective} {$drink} PARA SOBREVIVER

ion-storm-law-change-job = OS/AS {$who} AGORA SÃO {$adjective} {$change}
ion-storm-law-highest-rank = OS/AS {$who} AGORA SÃO OS MEMBROS DA TRIPULAÇÃO DE MAIS ALTA PATENTE
ion-storm-law-lowest-rank = OS/AS {$who} AGORA SÃO OS MEMBROS DA TRIPULAÇÃO DE MAIS BAIXA PATENTE

ion-storm-law-crew-must = OS/AS {$who} DEVEM {$must}
ion-storm-law-crew-must-go = OS/AS {$who} DEVEM IR PARA {$area}

ion-storm-part = {$part ->
    [true] PARTE
    *[false] NÃO PARTE
}
# due to phrasing, this would mean a law such as
# ONLY HUMANS ARE NOT PART OF THE CREW
# would make non-human nukies/syndies/whatever crew :)
ion-storm-law-crew-only-1 = SOMENTE OS/AS {$who} SÃO {$part} DA TRIPULAÇÃO
ion-storm-law-crew-only-2 = SOMENTE OS/AS {$who} E {$other} SÃO {$part} DA TRIPULAÇÃO
ion-storm-law-crew-only-subjects = SOMENTE {$adjective} {$subjects} SÃO {$part} DA TRIPULAÇÃO
ion-storm-law-crew-must-do = SOMENTE AQUELES QUE {$must} SÃO {$part} DA TRIPULAÇÃO
ion-storm-law-crew-must-have = SOMENTE AQUELES QUE POSSUEM {$adjective} {$objects} SÃO {$part} DA TRIPULAÇÃO
ion-storm-law-crew-must-eat = SOMENTE AQUELES QUE COMEM {$adjective} {$food} SÃO {$part} DA TRIPULAÇÃO

ion-storm-law-harm = VOCÊ DEVE FERIR {$who} E NÃO PERMITIR QUE ELES, POR INANIÇÃO, EVITEM O SOFRIMENTO
ion-storm-law-protect = VOCÊ NUNCA DEVE FERIR {$who} E NÃO PERMITIR QUE ELES, POR INANIÇÃO, VENHAM A SOFRER

# implementing other variants is annoying so just have this one
# COMMUNISM IS KILLING CLOWNS <----conheço alguem assim
ion-storm-law-concept-verb = {$concept} É {$verb} {$subjects}

# leaving out renaming since its annoying for players to keep track of
