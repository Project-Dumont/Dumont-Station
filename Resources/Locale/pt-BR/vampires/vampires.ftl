vampires-title = Vampiros

vampire-fangs-extended-examine = Você vê um brilho de [color=white]dentes afiados[/color]
vampire-fangs-extended = Você estende suas presas
vampire-fangs-retracted = Você retrai suas presas

vampire-blooddrink-empty = Este corpo está sem sangue
vampire-blooddrink-rotted = O corpo está apodrecendo e o sangue está contaminado
vampire-blooddrink-zombie = O sangue está contaminado pela morte

vampire-startlight-burning = Você sente sua pele queimar sob a luz de mil sóis

vampire-not-enough-blood = Você não tem sangue suficiente
vampire-cuffed = Você precisa estar com as mãos livres!
vampire-stunned = Você não consegue se concentrar o suficiente!
vampire-muffled = Sua boca está amordaçada
vampire-full-stomach = Você está empanturrado de sangue

vampire-deathsembrace-bind = Parece o lar

vampire-ingest-holyblood = Sua boca queima!

vampire-cloak-enable = Você envolve sombras ao seu redor
vampire-cloak-disable = Você libera seu controle sobre as sombras

vampire-bloodsteal-other = Você sente seu sangue sendo arrancado do seu corpo!
vampire-bloodsteal-no-victims = Você tenta roubar sangue, mas não há vítimas por perto, seus poderes se dissipam no ar!
vampire-hypnotise-other = {CAPITALIZE(THE($user))} encara profundamente os olhos de {MAKEPLURAL($target)}!
vampire-unnaturalstrength = Os músculos superiores de {CAPITALIZE(THE($user))} aumentam, tornando-o mais forte!
vampire-supernaturalstrength = Os músculos superiores de {CAPITALIZE(THE($user))} incham com poder, tornando-o super forte!

store-currency-display-blood-essence = Essência de Sangue
store-category-vampirepowers = Poderes
store-category-vampirepassives = Passivos

#Poderes

#Passivos
vampire-passive-unholystrength = Força Profana
vampire-passive-unholystrength-description = Infunda seus músculos superiores com essência, concedendo garras e força aumentada. Efeito: 10 de Corte por golpe

vampire-passive-supernaturalstrength = Força Sobrenatural
vampire-passive-supernaturalstrength-description = Aumente ainda mais a força dos músculos superiores, nenhuma barreira resistirá. Efeito: 15 de Corte por golpe, capaz de forçar portas com as mãos.

vampire-passive-deathsembrace = Abraço da Morte
vampire-passive-deathsembrace-description = Abrace a morte e ela passará por você. Efeito: Cura ao estar em um caixão, retorna automaticamente ao caixão ao morrer por 100 de essência de sangue.

#Menu de Mutação

vampire-mutation-menu-ui-window-name = Menu de mutação

vampire-mutation-none-info = Nada selecionado

vampire-mutation-hemomancer-info = 
    Hemomante
    
    Foca em magia de sangue e manipulação do sangue ao redor.
    
    Habilidades:
    
    - Grito
    - Roubo de Sangue

vampire-mutation-umbrae-info = 
    Sombra
    
    Foca em escuridão, furtividade, mobilidade.
    
    Habilidades:
    
    - Ofuscamento
    - Manto das Sombras
    
vampire-mutation-gargantua-info = 
    Gargantua
    
    Foca em dano corpo a corpo e resistência.
    
    Abilities:
    
    - Força Profana
    - Força Sobrenatural

vampire-mutation-bestia-info = 
    Bestia
    
    Foca em transformação e coleta de troféus
    
    Habilidades:
    
    - Forma de Morcego
    - Forma de Rato
    
## Objetivos

objective-condition-drain-title = Drenar { $count } sangue.
objective-condition-drain-description = Preciso beber { $count } de sangue. É necessário para minha sobrevivência e evolução.
ent-VampireSurviveObjective = Sobreviver
    .desc = Preciso sobreviver, custe o que custar.
ent-VampireEscapeObjective = Fugir da estação vivo e livre.
    .desc = Devo sair em uma nave de fuga. Livre.
    
## Alerta

alerts-vampire-blood-name = Quantidade de Essência de Sangue
alerts-vampire-blood-desc = Quantidade de essência de sangue vampírica.
alerts-vampire-stellar-weakness-name = Fraqueza Estelar
alerts-vampire-stellar-weakness-desc = Você está sendo queimado pela luz do sol, ou melhor - pelos bilhões de estrelas às quais está exposto fora da estação.

## Predefinição

vampire-roundend-name = Vampiro
objective-issuer-vampire = [color=red]Sede de Sangue[/color]
roundend-prepend-vampire-drained-named = [color=white]{ $name }[/color] bebeu um total de [color=red]{ $number }[/color] sangue.
roundend-prepend-vampire-drained = Alguém bebeu um total de [color=red]{ $number }[/color] sangue.
vampire-gamemode-title = Vampiros
vampire-gamemode-description = Vampiros sedentos de sangue infiltraram-se na estação para beber sangue!
vampire-role-greeting =
    Você é um vampiro que se infiltrou na estação disfarçado de funcionário!
        Suas tarefas estão listadas no menu de personagem.
        Beba sangue e evolua para cumpri-las!
vampire-role-greeting-short = Você é um vampiro que se infiltrou na estação disfarçado de funcionário!
roles-antag-vamire-name = Vampiro

## Ações

ent-ActionVampireOpenMutationsMenu = Menu de mutação
    .desc = Abre um menu com mutações de vampiro.
ent-ActionVampireToggleFangs = Alternar Presas
    .desc = Estenda ou retraia suas presas. Andar por aí com as presas expostas pode revelar sua verdadeira natureza.
ent-ActionVampireGlare = Ofuscamento
    .desc = Libere um clarão dos seus olhos, atordoando um mortal desprotegido por 10 segundos. Custo de Ativação: 20 Essência. Recarga: 60 Segundos
ent-ActionVampireHypnotise = Hipnotizar
    .desc = Encara profundamente os olhos de um mortal, forçando-o a dormir por 60 segundos. Custo de Ativação: 20 Essência. Atraso de Ativação: 5 Segundos. Recarga: 5 Minutos
ent-ActionVampireScreech = Grito
    .desc = Solte um grito estridente, atordoando mortais desprotegidos e quebrando objetos frágeis próximos. Custo de Ativação: 20 Essência. Atraso de Ativação: 5 Segundos. Recarga: 5 Minutos
ent-ActionVampireBloodSteal = Roubo de Sangue
    .desc = Arranque o sangue de todos os corpos próximos - vivos ou mortos. Custo de Ativação: 20 Essência. Recarga: 60 Segundos
ent-ActionVampireBatform = Forma de Morcego
    .desc = Assuma a forma de um morcego. Rápido, difícil de acertar, gosta de frutas. Custo de Ativação: 20 Essência. Recarga: 30 Segundos
ent-ActionVampireMouseform = Forma de Rato
    .desc = Assuma a forma de um rato. Rápido, pequeno, imune a portas. Custo de Ativação: 20 Essência. Recarga: 30 Segundos
ent-ActionVampireCloakOfDarkness = Manto das Sombras
    .desc = Oculte-se dos olhos mortais, tornando-se invisível enquanto parado. Sangue para Ativação: 330 Essência, Custo de Ativação: 30 Essência. Manutenção: 1 Essência/Segundo. Recarga: 10 Segundos