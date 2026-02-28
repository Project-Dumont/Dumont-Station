shared-solution-container-component-on-examine-main-text = Contém uma [color={$color}]{$desc}[/color] { $chemCount ->
    [1] produto químico.
   *[other] mistura de produtos químicos.
}

examinable-solution-has-recognizable-chemicals = Você pode reconhecer {$recognizedString} na solução.
examinable-solution-recognized = [color={$color}]{$chemical}[/color]

examinable-solution-on-examine-volume = A solução contida está { $fillLevel ->
    [exact] segurando [color=white]{$current}/{$max}u[/color].
   *[other] [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}

examinable-solution-on-examine-volume-no-max = A solução contida está { $fillLevel ->
    [exact] segurando [color=white]{$current}u[/color].
   *[other] [bold]{ -solution-vague-fill-level(fillLevel: $fillLevel) }[/bold].
}

examinable-solution-on-examine-volume-puddle = A poça está { $fillLevel ->
    [exact] segurando [color=white]{$current}u[/color].
    [full] gigante e transbordando!
    [mostlyfull] gigante e transbordando!
    [halffull] funda e fluindo.
    [halfempty] bem funda.
   *[mostlyempty] se juntando.
    [empty] formando várias pocinhas.
}

-solution-vague-fill-level =
    { $fillLevel ->
        [full] [color=white]Cheia[/color]
        [mostlyfull] [color=#DFDFDF]Quase Cheia[/color]
        [halffull] [color=#C8C8C8]Metade Cheia[/color]
        [halfempty] [color=#C8C8C8]Metade Vazia[/color]
        [mostlyempty] [color=#A4A4A4]Quase Vazia[/color]
       *[empty] [color=gray]Vazia[/color]
    }
