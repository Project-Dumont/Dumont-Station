lathe-menu-title = Menu do Torno
lathe-menu-queue = Fila
lathe-menu-server-list = Servidores
lathe-menu-sync = Sincronizar
lathe-menu-search-designs = Procurar receitas
lathe-menu-category-all = Todos
lathe-menu-search-filter = Filtro:
lathe-menu-amount = Quantidade:
lathe-menu-recipe-count = { $count ->
    [1] { $count } Receita
    *[other] { $count } Receitas
}
lathe-menu-reagent-slot-examine = Isso tem um espaço para béqueres ao lado.
lathe-reagent-dispense-no-container = Os líquidos caem { PREPOSICAO-DE($name) } { $name } no chão!
lathe-menu-result-reagent-display = {$reagent} ({$amount}u)
lathe-menu-material-display = { $material } ({ $amount })
lathe-menu-tooltip-display = { $amount } de { $material }
lathe-menu-description-display = [italic]{$description}[/italic]
lathe-menu-material-amount = { $amount ->
    [1] {NATURALFIXED($amount, 2)} {$unit}
    *[other] {NATURALFIXED($amount, 2)} {MAKEPLURAL($unit)}
}
lathe-menu-material-amount-missing = { $amount ->
    [1] {NATURALFIXED($amount, 2)} {$unit} de {$material} ([color=red]{NATURALFIXED($missingAmount, 2)} {$unit} faltando[/color])
    *[other] {NATURALFIXED($amount, 2)} {MAKEPLURAL($unit)} de {$material} ([color=red]{NATURALFIXED($missingAmount, 2)} {MAKEPLURAL($unit)} faltando[/color])
}
lathe-menu-no-materials-message = Nenhum material carregado.
lathe-menu-fabricating-message = Fabricando...
lathe-menu-materials-title = Materiais
lathe-menu-queue-title = Fila de construção
lathe-menu-queue-reset-title = Limpar a fila
lathe-menu-queue-reset-material-overflow = O torno está cheio.
