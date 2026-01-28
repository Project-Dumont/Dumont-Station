# SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
# SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
# SPDX-FileCopyrightText: 2024 lanse12 <cloudability.ez@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 GitHubUser53123 <110841413+GitHubUser53123@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
# SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

ent-SpawnPointGhostBlob = Gerador de Blob
    .suffix = DEBUG, Gerador de Papel Fantasma
    .desc = { ent-MarkerBase.desc }
ent-MobBlobPod = Blob Drop
    .desc = Um lutador de blob comum.
ent-MobBlobBlobbernaut = Blobbernaut
    .desc = Um lutador de blob de elite.
ent-BaseBlob = blob básico.
    .desc = { "" }
ent-NormalBlobTile = Tile Blob Comum
    .desc = Uma parte comum do blob necessária para a construção de tiles mais avançados.
ent-CoreBlobTile = Núcleo do Blob
    .desc = O órgão mais importante do blob. Destruir o núcleo cessará a infecção.
ent-FactoryBlobTile = Fábrica de Blob
    .desc = Gera Blob Drops e Blobbernauts ao longo do tempo.
ent-ResourceBlobTile = Blob de Recursos
    .desc = Produz recursos para o blob.
ent-NodeBlobTile = Nodo do Blob
    .desc = Uma versão mini do núcleo que permite colocar tiles especiais de blob ao seu redor.
ent-StrongBlobTile = Tile Blob Reforçado
    .desc = Uma versão reforçada do tile comum. Não permite passagem de ar e protege contra dano bruto.
ent-ReflectiveBlobTile = Tiles Reflexivos de Blob
    .desc = Reflete lasers, mas não protege tão bem contra dano bruto.
    .desc = { "" }
objective-issuer-blob = Blob

ghost-role-information-blobbernaut-name = Blobbernaut
ghost-role-information-blobbernaut-description = Você é um Blobbernaut. Deve defender o núcleo do blob. Use + ou +e no chat para falar na Blobmind.

ghost-role-information-blob-name = Blob
ghost-role-information-blob-description = Você é a Infecção Blob. Consuma a estação.

roles-antag-blob-name = Blob
roles-antag-blob-objective = Alcance massa crítica.

guide-entry-blob = Blob

# Popups
blob-target-normal-blob-invalid = Tipo de blob incorreto, selecione um blob normal.
blob-target-factory-blob-invalid = Tipo de blob incorreto, selecione um blob fábrica.
blob-target-node-blob-invalid = Tipo de blob incorreto, selecione um nodo de blob.
blob-target-close-to-resource = Muito próximo de outro blob de recurso.
blob-target-nearby-not-node = Nenhum nodo ou blob de recurso próximo.
blob-target-close-to-node = Muito próximo de outro nodo.
blob-target-already-produce-blobbernaut = Esta fábrica já produziu um blobbernaut.
blob-cant-split = Você não pode dividir o núcleo do blob.
blob-not-have-nodes = Você não possui nodos.
blob-not-enough-resources = Recursos insuficientes.
blob-help = Só Deus pode te ajudar.
blob-swap-chem = Em desenvolvimento.
blob-mob-attack-blob = Você não pode atacar um blob.
blob-get-resource = +{ $point }
blob-spent-resource = -{ $point }
blobberaut-not-on-blob-tile = Você está morrendo fora dos tiles de blob.
carrier-blob-alert = Você tem { $second } segundos antes da transformação.

blob-mob-zombify-second-start = { $pod } começa a transformá-lo em zumbi.
blob-mob-zombify-third-start = { $pod } começa a transformar { $target } em zumbi.

blob-mob-zombify-second-end = { $pod } transforma você em zumbi.
blob-mob-zombify-third-end = { $pod } transforma { $target } em zumbi.

blobberaut-factory-destroy = fábrica destruída
blob-target-already-connected = já conectado

# UI
blob-chem-swap-ui-window-name = Trocar químicos
blob-chem-reactivespines-info = Espinhos Reativos
                                Causam 25 de dano bruto.
blob-chem-blazingoil-info = Óleo Incandescente
                            Causa 15 de dano por queimadura e incendeia alvos.
                            Torna você vulnerável à água.
blob-chem-regenerativemateria-info = Matéria Regenerativa
                                    Causa 6 de dano bruto e 15 de dano por toxina.
                                    O núcleo do blob regenera saúde 10 vezes mais rápido que o normal e gera 1 recurso extra.
blob-chem-explosivelattice-info = Lattice Explosivo
                                    Causa 5 de dano por queimadura e explode o alvo, causando 10 de dano bruto.
                                    Esporos explodem ao morrer.
                                    Você se torna imune a explosões.
                                    Recebe 50% mais dano de queimaduras e choque elétrico.
blob-chem-electromagneticweb-info = Rede Eletromagnética
                                    Causa 20 de dano por queimadura, 20% de chance de gerar pulso EMP ao atacar.
                                    Tiles de blob geram pulso EMP quando destruídos.
                                    Você recebe 25% mais dano bruto e de calor.

blob-alert-out-off-station = O blob foi removido por estar fora da estação!

# Anúncios
blob-alert-recall-shuttle = O transporte de emergência não pode ser enviado enquanto houver risco biológico nível 5 na estação.
blob-alert-detect = Surto confirmado de risco biológico nível 5 a bordo da estação. Todo o pessoal deve conter o surto.
blob-alert-critical = Nível de risco biológico crítico, códigos de autenticação nuclear enviados à estação. Comando Central ordena que qualquer pessoal restante ative o mecanismo de autodestruição.
blob-alert-critical-NoNukeCode = Nível de risco biológico crítico. Comando Central ordena que qualquer pessoal restante busque abrigo e aguarde resgate.
blob-alert-shuttle-arrived = Risco biológico detectado a bordo. Todo o pessoal deve evacuar imediatamente.

# Ações
blob-teleport-to-node-action-name = Teleportar para Nodo (0)
blob-teleport-to-node-action-desc = Teleporta você para um nodo de blob aleatório.
blob-help-action-name = Ajuda
blob-help-action-desc = Obtenha informações básicas sobre como jogar como blob.

# Papel Fantasma
blob-carrier-role-name = Portador de Blob
blob-carrier-role-desc = Uma criatura infectada pelo blob.
blob-carrier-role-rules = Você é um antagonista. Tem 10 minutos antes de se transformar em blob.
                        Use esse tempo para encontrar um local seguro na estação. Lembre-se que você será muito fraco logo após a transformação.
blob-carrier-role-greeting = Você é um portador de Blob. Encontre um local isolado na estação e transforme-se em Blob. Transforme a estação em uma massa e seus habitantes em seus servos. Nós somos todos Blobs.

# Verbos
blob-pod-verb-zombify = Transformar em Zumbi
blob-verb-upgrade-to-strong = Evoluir para Blob Reforçado
blob-verb-upgrade-to-reflective = Evoluir para Blob Reflexivo
blob-verb-remove-blob-tile = Remover Blob

# Alertas
blob-resource-alert-name = Recursos do Núcleo
blob-resource-alert-desc = Seus recursos produzidos pelos blobs de núcleo e recurso. Use-os para expandir e criar blobs especiais.
blob-health-alert-name = Saúde do Núcleo
blob-health-alert-desc = Saúde do seu núcleo. Você morrerá se chegar a zero.

# Saudação
blob-role-greeting =
    Você é um blob - uma criatura parasita espacial capaz de destruir estações inteiras.
        Seu objetivo é sobreviver e crescer o máximo possível.
        Você é quase invulnerável a danos físicos, mas calor ainda pode te ferir.
        Use Alt+LMB para evoluir tiles de blob normais para reforçados e de reforçados para reflexivos.
        Certifique-se de colocar blobs de recurso para gerar recursos.
        Lembre-se que blobs de recurso e fábricas só funcionarão próximos a nodos ou núcleos.
        Você pode usar + ou +e no chat para usar a Blobmind e falar com seus minions.
blob-zombie-greeting = Você foi infectado e criado por um esporo de blob. Agora deve ajudar o blob a dominar a estação. Use +e no chat para falar na Blobmind.

# Fim de rodada
blob-round-end-result =
    { $blobCount ->
        [one] Houve uma infecção de blob.
        *[other] Houve {$blobCount} blobs.
    }

blob-user-was-a-blob = [color=gray]{$user}[/color] era um blob.
blob-user-was-a-blob-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) era um blob.
blob-was-a-blob-named = [color=White]{$name}[/color] era um blob.

preset-blob-objective-issuer-blob = [color=#33cc00]Blob[/color]

blob-user-was-a-blob-with-objectives = [color=gray]{$user}[/color] era um blob com os seguintes objetivos:
blob-user-was-a-blob-with-objectives-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color]) era um blob com os seguintes objetivos:
blob-was-a-blob-with-objectives-named = [color=White]{$name}[/color] era um blob com os seguintes objetivos:

# Objetivos
objective-condition-blob-capture-title = Dominar a estação
objective-condition-blob-capture-description = Seu único objetivo é dominar toda a estação. Você precisa ter pelo menos {$count} tiles de blob.
objective-condition-success = { $condition } | [color={ $markupColor }]Sucesso![/color]
objective-condition-fail = { $condition } | [color={ $markupColor }]Falha![/color] ({ $progress }%)

# Verbos de Admin
admin-verb-make-blob = Transformar o alvo em portador de Blob.
admin-verb-text-make-blob = Transformar em Portador de Blob

# Idioma
language-Blob-name = Blob
chat-language-Blob-name = Blob
language-Blob-description = Bleeb bob! Blob blob!
