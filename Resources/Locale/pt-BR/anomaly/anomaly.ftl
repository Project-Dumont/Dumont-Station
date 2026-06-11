anomaly-component-contact-damage = A anomalia torra fora sua pele!

anomaly-vessel-component-anomaly-assigned = Anomalia atribuida a vassalo.
anomaly-vessel-component-not-assigned = Este vassalo não está atrobuído a nenhuma anomalia. Tente usar um scanner nele.
anomaly-vessel-component-assigned = Este vassalo está atualmente atribuído a uma anomalia.

anomaly-particles-delta = Partículas Delta
anomaly-particles-epsilon = Partículas Epsilon
anomaly-particles-zeta = Partículas Zeta
anomaly-particles-omega = Partículas Omega
anomaly-particles-sigma = Partículas Sigma


anomaly-scanner-component-scan-complete = Escan completao!

anomaly-scanner-ui-title = escaner de anomalia
anomaly-scanner-no-anomaly = Nenhuma anomalia escaneda atualmente.
anomaly-scanner-severity-percentage = Severidade atual: [color=gray]{$percent}[/color]
anomaly-scanner-severity-percentage-unknown = Severidade atual: [color=red]ERROR[/color]
anomaly-scanner-stability-low = Estado atual da anomalia: [color=gold]Decaindo[/color]
anomaly-scanner-stability-medium = Estado atual da anomalia: [color=forestgreen]Estável[/color]
anomaly-scanner-stability-high = Estado atual da anomalia: [color=crimson]Crescendo[/color]
anomaly-scanner-stability-unknown = Estado atual da anomalia: [color=red]ERROR[/color]																					
anomaly-scanner-point-output = Ponto de saída: [color=gray]{$point}[/color]
anomaly-scanner-point-output-unknown = Ponto de saída: [color=red]ERROR[/color]
anomaly-scanner-particle-readout = Análise de Reação a Partículas:			
anomaly-scanner-particle-danger = - [color=crimson]Tipo perigoso:[/color] {$type}
anomaly-scanner-particle-unstable = - [color=plum]Tipo instável:[/color] {$type}
anomaly-scanner-particle-containment = - [color=goldenrod]Tipo de contensão:[/color] {$type}
anomaly-scanner-particle-transformation = - [color=#6b75fa]Tipo de Transformação:[/color] {$type}
anomaly-scanner-particle-danger-unknown = - [color=crimson]Tipo de Perigo:[/color] [color=red]ERRO[/color]
anomaly-scanner-particle-unstable-unknown = - [color=plum]Tipo de Instabilidade:[/color] [color=red]ERRO[/color]
anomaly-scanner-particle-containment-unknown = - [color=goldenrod]Tipo de Contenção:[/color] [color=red]ERRO[/color]
anomaly-scanner-particle-transformation-unknown = - [color=#6b75fa]Tipo de Transformação:[/color] [color=red]ERRO[/color]
anomaly-scanner-pulse-timer = Tempo até próximo pulso: [color=gray]{$time}[/color]

anomaly-gorilla-core-slot-name = Núcleo de anomalia
anomaly-gorilla-charge-none = Não possui [bold]núcleo de anomalia[/bold] dentro dele.
anomaly-gorilla-charge-limit = Tem [color={$count ->
    [3]green
    [2]yellow
    [1]orange
    [0]red
    *[other]purple
}]{$count} {$count ->
    [one]carga
    *[other]cargas
}[/color] restante.
anomaly-gorilla-charge-infinite = Possui [color=gold]cargas infinitas[/color]. [italic]Por enquanto...[/italic]

anomaly-sync-connected = Anomalia anexada com sucesso
anomaly-sync-disconnected = A conexão com a anomalia foi perdida!
anomaly-sync-no-anomaly = Nenhuma anomalia no alcance.
anomaly-sync-examine-connected = Está [color=darkgreen]anexado[/color] a uma anomalia.
anomaly-sync-examine-not-connected = [color=darkred]não está anexado[/color] a uma anomalia.
anomalia-sync-connect-verb-text = Anexar anomalia
anomaly-sync-connect-verb-message = Anexe uma anomalia próxima a {THE($machine)}.

anomaly-generator-ui-title = gerador de anomalia
anomaly-generator-fuel-display = Combustível:
anomaly-generator-cooldown = Cooldown: [color=gray]{$time}[/color]
anomaly-generator-no-cooldown = Cooldown: [color=gray]Completo[/color]
anomaly-generator-yes-fire = Estado: [color=forestgreen]Pronto[/color]
anomaly-generator-no-fire = Estado: [color=crimson]Não pronto[/color]
anomaly-generator-generate = Gerar Anomalia
anomaly-generator-charges = {$charges ->
    [one] {$charges} carga
    *[other] {$charges} cargas
}
anomaly-generator-announcement = Uma anomalia foi gerada!

anomaly-command-pulse = Pulsa um alvo de anomalia
anomaly-command-supercritical = Faz com que um alvo de anomalia se torne supercrítica

# Flavor text on the footer
anomaly-generator-flavor-left = Anomalias podem surgir dentro do operador.
anomaly-generator-flavor-right = v1.1

anomaly-behavior-unknown = [color=red]ERRO. Não pode ser lido.[/color]

anomaly-behavior-title = análise de desvio de comportamento:
anomaly-behavior-point =[color=gold]A anomalia produz {$mod}% dos pontos[/color]

anomaly-behavior-safe = [color=forestgreen]A anomalia é extremamente estável. Pulsações extremamente raras.[/color]
anomaly-behavior-slow = [color=forestgreen]A frequência das pulsações é muito menos frequente.[/color]
anomaly-behavior-light = [color=forestgreen]A potência das pulsações é significativamente reduzida.[/color]
anomaly-behavior-balanced = Nenhum desvio de comportamento detectado.
anomaly-behavior-delayed-force = A frequência das pulsações é muito reduzida, mas sua potência é aumentada.
anomaly-behavior-rapid = A frequência da pulsação é muito maior, mas sua intensidade é atenuada.
anomaly-behavior-reflect = Um revestimento protetor foi detectado.
anomaly-behavior-nonsensivity = Uma reação fraca a partículas foi detectada.
anomaly-behavior-sensivity = Uma reação amplificada a partículas foi detectada.
anomaly-behavior-secret = Interferência detectada. Alguns dados não podem ser lidos.
anomaly-behavior-inconstancy = [color=crimson]Foi detectada impermanência. Os tipos de partículas podem mudar ao longo do tempo.[/color]
anomaly-behavior-fast = [color=crimson]A frequência da pulsação está fortemente aumentada.[/color]
anomaly-behavior-strenght = [color=crimson]A potência da pulsação está significativamente aumentada.[/color]
anomaly-behavior-moving = [color=crimson]Instabilidade de coordenadas foi detectada.[/color]

# Dumont Station — Gerador Avançado de Anomalias

advanced-anomaly-generator-ui-title = Gerador Avançado de Anomalias
advanced-anomaly-generator-ui-anomaly = Anomalia
advanced-anomaly-generator-ui-resources = Recursos
advanced-anomaly-generator-ui-map = Seletor de tile da estação
advanced-anomaly-generator-ui-use-machine-tile = Usar tile da máquina

advanced-anomaly-generator-ui-selected-tile = Tile selecionado: [color=cyan]{ $x }, { $y }[/color]
advanced-anomaly-generator-ui-generate = Gerar anomalia
advanced-anomaly-generator-ui-anomaly-entry = { $name } ({ $cost } pesquisa)
advanced-anomaly-generator-ui-log = Registro de atividade
advanced-anomaly-generator-ui-card-research = [color=cyan]{ $cost }[/color] [color=#aaaaaa]pesquisa[/color]
advanced-anomaly-generator-ui-card-plasma = [color=#4fc3f7]{ $cost }[/color] [color=#aaaaaa]plasma sólido[/color]
advanced-anomaly-generator-ui-resource-status = Plasma: [color=cyan]{ $plasma }[/color] / [color=cyan]{ $plasmaCost }[/color]
    Pontos de pesquisa: [color=cyan]{ $points }[/color]
advanced-anomaly-generator-ui-cost = Protótipo: [color=gray]{ $prototype }[/color]
    Custo de pesquisa: [color=cyan]{ $cost }[/color]
    Custo de plasma: [color=cyan]{ $plasma }[/color]
advanced-anomaly-generator-ui-no-message = Aguardando uma solicitação de geração.
advanced-anomaly-generator-ui-no-anomaly = Nenhuma anomalia selecionada.
advanced-anomaly-generator-entry-pyroclastic = Anomalia piroclástica
advanced-anomaly-generator-entry-gravity = Anomalia gravitacional
advanced-anomaly-generator-entry-electricity = Anomalia elétrica
advanced-anomaly-generator-entry-ice = Anomalia de gelo
advanced-anomaly-generator-entry-flora = Anomalia floral
advanced-anomaly-generator-entry-bluespace = Anomalia bluespace
advanced-anomaly-generator-entry-tech = Anomalia tecnológica
advanced-anomaly-generator-entry-rock = Anomalia de rocha
advanced-anomaly-generator-entry-santa = Anomalia santa
advanced-anomaly-generator-entry-shadow = Anomalia sombria
advanced-anomaly-generator-error-unpowered = O gerador avançado de anomalias não está energizado.
advanced-anomaly-generator-error-invalid-anomaly = A anomalia selecionada não está configurada ou não existe mais.
advanced-anomaly-generator-error-plasma = Plasma sólido insuficiente: { $available } / { $needed }.
advanced-anomaly-generator-error-no-server = O gerador não está vinculado a um servidor de pesquisa.
advanced-anomaly-generator-error-research = Pontos de pesquisa insuficientes: { $available } / { $needed }.
advanced-anomaly-generator-error-no-station = Nenhuma grade da estação foi encontrada neste mapa.
advanced-anomaly-generator-error-wrong-grid = O gerador deve estar na grade principal da estação.
advanced-anomaly-generator-error-invalid-location = O tile escolhido não é um tile interior válido da estação.
advanced-anomaly-generator-error-blocked-location = O tile escolhido está bloqueado por um objeto ancorado intransponível.
advanced-anomaly-generator-success = { $anomaly } gerada no tile { $x }, { $y } da estação.
advanced-anomaly-generator-announce = { $anomaly } gerada por { $user } perto de { $location }.
advanced-anomaly-generator-announce-unknown-user = um operador desconhecido
advanced-anomaly-generator-error-cooldown = O gerador está recarregando. Tempo restante: { $time }.