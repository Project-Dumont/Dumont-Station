### Loc for the pneumatic cannon.

pneumatic-cannon-component-verb-gas-tank-name = Ejetar tanque de gás
pneumatic-cannon-component-verb-eject-items-name = Ejetar tudo

## Shown when inserting items into it

pneumatic-cannon-component-insert-item-success = Você insere {ARTIGO-O($item)} {$item} {PREPOSICAO-EM($cannon)} {$cannon}.
pneumatic-cannon-component-insert-item-failure = Você não consegue encaixar {ARTIGO-O($item)} {$item} {PREPOSICAO-EM($cannon)} {$cannon}.

## Shown when trying to fire, but no gas

pneumatic-cannon-component-fire-no-gas = {CAPITALIZE(ARTIGO-O($cannon))} {$cannon} estala, mas nenhum gás sai.

## Shown when changing the fire mode or power.

pneumatic-cannon-component-change-fire-mode = { $mode ->
    [All] Você solta as válvulas para soltar tudo de uma vez.
    *[Single] Você aperta as válvulas para soltar uma coisa de cada vez.
}

pneumatic-cannon-component-change-power = { $power ->
    [High] Você seleciona o limitador para energia alta. parece estar muito energizado...
    [Medium] Você seleciona o limitador para energia média.
    *[Low] Você seleciona o limitador para energia baixa.
}

## Shown when inserting/removing the gas tank.

pneumatic-cannon-component-gas-tank-insert = Você encaixa {ARTIGO-UM($tank)} {$tank} {PREPOSICAO-EM($cannon)} {$cannon}.
pneumatic-cannon-component-gas-tank-remove = Você tira {ARTIGO-UM($tank)} {$tank} {PREPOSICAO-DE($cannon)} {$cannon}.
pneumatic-cannon-component-gas-tank-none = Não há um tanque de gás {PREPOSICAO-EM($cannon)} {$cannon}!

## Shown when ejecting every item from the cannon using a verb.

pneumatic-cannon-component-ejected-all = Você ejeta tudo {PREPOSICAO-DE($cannon)} {$cannon}.

## Shown when being stunned by having the power too high.

pneumatic-cannon-component-power-stun = A pura força {PREPOSICAO-DE($cannon)} {$cannon} te derruba!
