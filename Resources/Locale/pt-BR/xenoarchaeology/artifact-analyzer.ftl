analysis-console-menu-title = console de análise
analysis-console-server-list-button = Lista de servidores
analysis-console-extract-button = Extrair

analysis-console-info-no-scanner = Nenhum analisador conectado! Conecte um usando uma multiferramenta.
analysis-console-info-no-artifact = Nenhum artefato presente! Coloque um na plataforma para ver suas informações.
analysis-console-info-ready = Sistemas operacionais. Pronto para digitalizar.

analysis-console-no-node = Selecione um nó para visualizar
analysis-console-info-id = NODE_ID:
analysis-console-info-id-value = [font="Monospace" size=11][color=yellow]{$id}[/color][/font]
analysis-console-info-class = [font="Monospace" size=11]Classe:[/font]
analysis-console-info-class-value = [font="Monospace" size=11]{$class}[/font]
analysis-console-info-locked = [font="Monospace" size=11]Status:[/font]
analysis-console-info-locked-value = [font="Monospace" size=11][color={ $state ->
    [0] red]Bloqueado
    [1] lime]Desbloqueado
    *[2] plum]Ativo
}[/color][/font]
analysis-console-info-durability = [font="Monospace" size=11]Durabilidade:[/font]
analysis-console-info-durability-value = [font="Monospace" size=11][color={$color}]{$current}/{$max}[/color][/font]
analysis-console-info-effect = [font="Monospace" size=11]Reações:[/font]
analysis-console-info-effect-value = [font="Monospace" size=11][color=gray]{ $state ->
    [true] {$info}
    *[false] Desbloqueie o nó para ganhar mais informação
}[/color][/font]
analysis-console-info-trigger = [font="Monospace" size=11]Estímulos:[/font]
analysis-console-info-triggered-value = [font="Monospace" size=11][color=gray]{$triggers}[/color][/font]

analysis-console-info-scanner = Escaneando...
analysis-console-info-scanner-paused = Pausado.
analysis-console-progress-text = {$seconds} {MANY("T-segundo", $seconds)}

analysis-console-extract-value = [font="Monospace" size=11][color=orange]Nó {$id} (+{$value})[/color][/font]
analysis-console-extract-none = [font="Monospace" size=11][color=orange] Nenhum nó desbloqueado contém pontos para extrair.[/color][/font]
analysis-console-extract-sum = [font="Monospace" size=11][color=orange]Total de pontos: {$value}[/color][/font]

analyzer-artifact-extract-popup = Energia resplandesce na superfície do artefato!
