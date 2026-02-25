command-list-langs-desc = Lista linguagens que sua entidade pode falar no momento atual.
command-list-langs-help = Uso: {$command}

command-saylang-desc = Enviar uma mensagem numa linguagem específica. Para selecionar uma linguagem, você pode usar o nome da linguagem ou sua posição na lista de linguagens.
command-saylang-help = Uso: {$command} <language id> <message>. Exemplo: {$command} TauCetiBasic "Olá Mundo!". Exemplo: {$command} 1 "Olá Mundo!"

command-language-select-desc = Selecione a linguagem falada atualmente por sua entidade. Você pode usar o nome da linguagem ou sua posição na lista de linguagens.
command-language-select-help = Uso: {$command} <language id>. Exemplo: {$command} 1. Exemplo: {$command} TauCetiBasic

command-language-spoken = Falado:
command-language-understood = Entendido:
command-language-current-entry = {$id}. {$language} - {$name} (atual)
command-language-entry = {$id}. {$language} - {$name}

command-language-invalid-number = O número da linguagem precisa ser entre 0 e {$total}. De outro modo, use o nome da linguagem.
command-language-invalid-language = A linguagem {$id} não existe ou você não pode falar ela.

# Toolshed

command-description-language-add = Adiciona uma nova linguagem para a entidade encadeada. Os últimos dois argumentos indicam se ela deveria ser falada/entendida. Exemplo: 'self language:add "Canilunzt" true true'
command-description-language-rm = Remove uma linguagem da entidade encadeada. Funciona similarmente a language:add. Exemplo: 'self language:rm "TauCetiBasic" true true'.
command-description-language-lsspoken = Lista todas as linguagens que a entidade encadeada consegue falar. Exemplo: 'self language:lsspoken'
command-description-language-lsunderstood = Lista todas as linguagens que a entidade encadeada consegue entender. Exemplo: 'self language:lssunderstood'

command-description-translator-addlang = Adiciona uma nova linguagem alvo para a entidade tradutora encadeada. Veja language:add para mais detalhes.
command-description-translator-rmlang = Remove uma linguagem alvo da entidade tradutora encadeada. Veja language:rm para mais detalhes..
command-description-translator-addrequired = Adiciona uma nova linguagem necessária para a entidade tradutora encadeada. Exemplo: 'ent 1234 translator:addrequired "TauCetiBasic"'
command-description-translator-rmrequired = Remove uma linguagem necessária de uma entidade tradutora encadeada. Exemplo: 'ent 1234 translator:rmrequired "TauCetiBasic"'
command-description-translator-lsspoken = Lista todas as linguagens faladas para a entidade tradutora encadeada. Exemplo: 'ent 1234 translator:lsspoken'
command-description-translator-lsunderstood = Lista todas as linguagens entendidas para a entidade tradutora encadeada. Exemplo: 'ent 1234 translator:lssunderstood'
command-description-translator-lsrequired = Lista todas as linguagens necessárias para a entidade tradutora encadeada. Exemplo: 'ent 1234 translator:lsrequired'

command-language-error-this-will-not-work = Isto não irá funcionar.
command-language-error-not-a-translator = Entidade {$entity} não é um tradutor.
