

armor-examine-stamina = - dano de [color=cyan]Stamina[/color] reduzido por [color=lightblue]{$num}%[/color].

armor-examine-cancel-delayed-knockdown = - [color=green]Completely cancels[/color] stun baton delayed knockdown.

armor-examine-modify-delayed-knockdown-delay =
    - { $deltasign ->
          [1] [color=green]Aumenta[/color]
          *[-1] [color=red]Diminui[/color]
      } o delay de knockdown do bastão de choque por [color=lightblue]{NATURALFIXED($amount, 2)} { $amount ->
          [1] segundo
          *[other] segundos
      }[/color].

armor-examine-modify-delayed-knockdown-time =
    - { $deltasign ->
          [1] [color=red]Aumenta[/color]
          *[-1] [color=green]Diminui[/color]
      } o knockdown do bastão de choque por [color=lightblue]{NATURALFIXED($amount, 2)} { $amount ->
          [1] segundo
          *[other] segundos
      }[/color].
