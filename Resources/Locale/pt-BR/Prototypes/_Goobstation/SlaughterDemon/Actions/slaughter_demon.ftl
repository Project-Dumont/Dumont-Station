# SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
# SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

- type: entity
  id: BloodCrawlAction
  name: Excursão Sangrenta
  description: APAREÇA OU DESAPAREÇA EM QUALQUER LUGAR QUE TENHA SANGUE.
  categories: [ HideSpawnMenu ]
  components:
  - type: Action
    icon:
      sprite: _Goobstation/SlaughterDemon/abilities.rsi
      state: crawl
    useDelay: 4
  - type: InstantAction
    event: !type:BloodCrawlEvent

- type: entity
  id: DemonicWhisperAction
  name: Sussurro Demoníaco
  description: ENVIE UMA MENSAGEM PARA A ALMA DELES, DIRETO DO INFERNO.
  categories: [ HideSpawnMenu ]
  components:
  - type: Action
    raiseOnUser: true
    icon:
      sprite: _Goobstation/SlaughterDemon/abilities.rsi
      state: whisper
    itemIconStyle: NoItem
  - type: TargetAction
    interactOnMiss: false
    range: 15
  - type: EntityTargetAction
    canTargetSelf: false
    event: !type:DemonicWhisperEvent
