station-ai-alarm-sender = Monitoramento Interno

station-ai-alarm-kind-atmos = atmosférico
station-ai-alarm-kind-fire = de incêndio

station-ai-alarm-warning = Alerta { $kind } em { $area }.
station-ai-alarm-danger = PERIGO { $kind } em { $area }.

station-ai-door-knock = { $who } está sem acesso a uma porta em { $area }.

# Monitor interno da IA
ai-alerts-ui-title = Monitoramento Interno
ai-alerts-ui-all-clear = Nenhum alerta ativo.
ai-alerts-ui-count = { $count ->
    [one] 1 alerta ativo.
   *[other] { $count } alertas ativos.
}
ai-alerts-ui-severity-warning = ALERTA
ai-alerts-ui-severity-danger = PERIGO
ai-alerts-ui-row = [color={ $color }]{ $severity }[/color] { $kind } — { $area }
ai-alerts-ui-row-door = [color={ $color }]PORTA[/color] { $who } — { $area }
ai-alerts-ui-jump = Ir

ent-ActionOpenAiAlertsMenu = Monitoramento Interno
    .desc = Alarmes de atmos e incêndio ao vivo, e quem ficou barrado numa porta. Dá pra pular pra cada um.
