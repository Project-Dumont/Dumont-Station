<<<<<<< HEAD
game-ticker-restart-round = Reiniciando a rodada...
game-ticker-start-round = A rodada está começando agora...
game-ticker-start-round-cannot-start-game-mode-fallback = Falha ao iniciar o modo {$failedGameMode}! Alternando para {$fallbackMode}...
game-ticker-start-round-cannot-start-game-mode-restart = Falha ao iniciar o modo {$failedGameMode}! Reiniciando a rodada...
game-ticker-start-round-invalid-map = O mapa selecionado {$map} é inadequado para o modo de jogo {$mode}. O modo de jogo pode não funcionar como esperado...
game-ticker-unknown-role = Desconhecido
game-ticker-delay-start = O início da rodada foi adiado por {$seconds} segundos.
game-ticker-pause-start = O início da rodada foi pausado.
game-ticker-pause-start-resumed = A contagem regressiva para o início da rodada foi retomada.
game-ticker-player-join-game-message = Bem-vindo à Space Station 14! Se esta é sua primeira vez jogando, leia as regras do jogo e não tenha medo de pedir ajuda no LOOC (OOC local) ou OOC (geralmente disponível apenas entre rodadas).
game-ticker-get-info-text = Olá e bem-vindo(a) à [color=#667C4D]Gaby Station![/color]
                            A rodada atual é: [color=white]#{$roundId}[/color]
                            O número atual de jogadores é: [color=white]{$playerCount}[/color]
                            O mapa atual é: [color=white]{$mapName}[/color]
                            O modo de jogo atual é: [color=white]{$gmTitle}[/color]
=======
# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 moonheart08 <moonheart08@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 pointer-to-null <91910481+pointer-to-null@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Myctai <108953437+Myctai@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Radosvik <65792927+Radosvik@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 ZeroDayDaemon <60460608+ZeroDayDaemon@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 theashtronaut <112137107+theashtronaut@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
# SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Rainfey <rainfey0+github@gmail.com>
# SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

game-ticker-restart-round = Restarting round...
game-ticker-start-round = The round is starting now...
game-ticker-start-round-cannot-start-game-mode-fallback = Failed to start {$failedGameMode} mode! Defaulting to {$fallbackMode}...
game-ticker-start-round-cannot-start-game-mode-restart = Failed to start {$failedGameMode} mode! Restarting round...
game-ticker-start-round-invalid-map = Selected map {$map} is inelligible for gamemode {$mode}. Gamemode may not function as intended...
game-ticker-unknown-role = Unknown
game-ticker-delay-start = Round start has been delayed for {$seconds} seconds.
game-ticker-pause-start = Round start has been paused.
game-ticker-pause-start-resumed = Round start countdown is now resumed.
game-ticker-player-join-game-message = Welcome to Space Station 14! If this is your first time playing, be sure to read the game rules, and don't be afraid to ask for help in LOOC (local OOC) or OOC (usually available only between rounds).
game-ticker-get-info-text = Hi and welcome to [color=white]Space Station 14![/color]
                            The current round is: [color=white]#{$roundId}[/color]
                            The current player count is: [color=white]{$playerCount}[/color]
                            The current map is: [color=white]{$mapName}[/color]
                            The current game mode is: [color=white]{$gmTitle}[/color]
>>>>>>> goob/master
                            >[color=yellow]{$desc}[/color]
game-ticker-get-info-preround-text = Olá e bem-vindo à [color=#667C4D]Gaby Station![/color]
                            A rodada atual é: [color=white]#{$roundId}[/color]
                            O número atual de jogadores é: [color=white]{$playerCount}[/color] ([color=white]{$readyCount}[/color] {$readyCount ->
                                [one] está
                                *[other] estão
                            } prontos)
                            O mapa atual é: [color=white]{$mapName}[/color]
                            O modo de jogo atual é: [color=white]{$gmTitle}[/color]
                            >[color=yellow]{$desc}[/color]
game-ticker-no-map-selected = [color=yellow]Mapa ainda não selecionado![/color]
game-ticker-player-no-jobs-available-when-joining = Ao tentar entrar no jogo, nenhum cargo estava disponível.

# Exibido no chat para administradores quando um jogador entra
player-join-message = Jogador {$name} entrou.
player-first-join-message = Jogador {$name} entrou pela primeira vez.

# Exibido no chat para administradores quando um jogador sai
player-leave-message = Jogador {$name} saiu.

latejoin-arrival-announcement = {$character} ({$job}) { CONJUGATE-HAVE($entity) } chegou à estação!
latejoin-arrival-announcement-special = {$job} {$character} a bordo!
latejoin-arrival-sender = Estação
latejoin-arrivals-direction = Uma nave que o transferirá para sua estação chegará em breve.
latejoin-arrivals-direction-time = Uma nave que o transferirá para sua estação chegará em {$time}.
latejoin-arrivals-dumped-from-shuttle = Uma força misteriosa impede você de sair com a nave de chegadas.
latejoin-arrivals-teleport-to-spawn = Uma força misteriosa teletransporta você para fora da nave de chegadas. Tenha um turno seguro!

preset-not-enough-ready-players = Não é possível iniciar {$presetName}. São necessários {$minimumPlayers} jogadores, mas temos apenas {$readyPlayersCount}.
preset-no-one-ready = Não é possível iniciar {$presetName}. Nenhum jogador está pronto.

game-run-level-PreRoundLobby = Lobby pré-rodada
game-run-level-InRound = Em rodada
game-run-level-PostRound = Pós-rodada
