objectives-round-end-result = {$count ->
    [one] Havia um {$agent}.
    *[other] Haviam {$count} {MAKEPLURAL($agent)}.
}

objectives-round-end-result-in-custody = {$custody} fora de {$count} {MAKEPLURAL($agent)} estavam sob custódia.

objectives-player-user-named = [color=white]{$name}[/color] ([color=gray]{$user}[/color])
objectives-player-user = [color=gray]{$user}[/color]
objectives-player-named = [color=white]{$name}[/color]

objectives-no-objectives = {$custody}{$title} era um {$agent}.
objectives-with-objectives = {$custody}{$title} era um {$agent} que tinha os seguintes objetivos:

objectives-objective-success = {$objective} | [color=green]Sucesso![/color] ({TOSTRING($progress, "P0")})
objectives-objective-partial-success = {$objective} | [color=yellow]Sucesso parcial![/color] ({TOSTRING($progress, "P0")})
objectives-objective-partial-failure = {$objective} | [color=orange]Falha parcial![/color] ({TOSTRING($progress, "P0")})
objectives-objective-fail = {$objective} | [color=red]Falha![/color] ({TOSTRING($progress, "P0")})

objectives-in-custody = [bold][color=red]| EM CUSTÓDIA | [/color][/bold]