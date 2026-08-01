station-ai-alarm-sender = Internal Monitoring

station-ai-alarm-kind-atmos = Atmospheric
station-ai-alarm-kind-fire = Fire

station-ai-alarm-warning = { $kind } warning in { $area }.
station-ai-alarm-danger = { $kind } DANGER in { $area }.

# AI internal alert monitor
ai-alerts-ui-title = Internal Monitoring
ai-alerts-ui-all-clear = No active alerts.
ai-alerts-ui-count = { $count ->
    [one] 1 active alert.
   *[other] { $count } active alerts.
}
ai-alerts-ui-severity-warning = WARNING
ai-alerts-ui-severity-danger = DANGER
ai-alerts-ui-row = [color={ $color }]{ $severity }[/color] { $kind } — { $area }
ai-alerts-ui-jump = Jump
