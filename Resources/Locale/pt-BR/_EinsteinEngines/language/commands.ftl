command-list-langs-desc = Lista os idiomas que sua entidade atual consegue falar no momento.
command-list-langs-help = Uso: {$command}

command-saylang-desc = Envia uma mensagem em um idioma específico. Para escolher um idioma, você pode usar o nome do idioma ou sua posição na lista.
command-saylang-help = Uso: {$command} <id do idioma> <mensagem>. Exemplo: {$command} TauCetiBasic "Olá Mundo!". Exemplo: {$command} 1 "Olá Mundo!"

command-language-select-desc = Seleciona o idioma atualmente falado pela sua entidade. Você pode usar o nome do idioma ou sua posição na lista.
command-language-select-help = Uso: {$command} <id do idioma>. Exemplo: {$command} 1. Exemplo: {$command} TauCetiBasic

command-language-spoken = Falado:
command-language-understood = Entendido:
command-language-current-entry = {$id}. {$language} - {$name} (atual)
command-language-entry = {$id}. {$language} - {$name}

command-language-invalid-number = O número do idioma deve estar entre 0 e {$total}. Alternativamente, use o nome do idioma.
command-language-invalid-language = O idioma {$id} não existe ou você não consegue falá-lo.

# Toolshed

command-description-language-add = Adiciona um novo idioma à entidade conectada. Os dois últimos argumentos indicam se ele deve ser falado/entendido. Exemplo: 'self language:add "Canilunzt" true true'
command-description-language-rm = Remove um idioma da entidade conectada. Funciona de forma semelhante ao language:add. Exemplo: 'self language:rm "TauCetiBasic" true true'.
command-description-language-lsspoken = Lista todos os idiomas que a entidade consegue falar. Exemplo: 'self language:lsspoken'
command-description-language-lsunderstood = Lista todos os idiomas que a entidade consegue entender. Exemplo: 'self language:lssunderstood'

command-description-translator-addlang = Adiciona um novo idioma alvo à entidade tradutora conectada. Veja language:add para detalhes.
command-description-translator-rmlang = Remove um idioma alvo da entidade tradutora conectada. Veja language:rm para detalhes.
command-description-translator-addrequired = Adiciona um novo idioma obrigatório à entidade tradutora conectada. Exemplo: 'ent 1234 translator:addrequired "TauCetiBasic"'
command-description-translator-rmrequired = Remove um idioma obrigatório da entidade tradutora conectada. Exemplo: 'ent 1234 translator:rmrequired "TauCetiBasic"'
command-description-translator-lsspoken = Lista todos os idiomas falados pela entidade tradutora conectada. Exemplo: 'ent 1234 translator:lsspoken'
command-description-translator-lsunderstood = Lista todos os idiomas entendidos pela entidade tradutora conectada. Exemplo: 'ent 1234 translator:lssunderstood'
command-description-translator-lsrequired = Lista todos os idiomas obrigatórios da entidade tradutora conectada. Exemplo: 'ent 1234 translator:lsrequired'

command-language-error-this-will-not-work = Isso não vai funcionar.
command-language-error-not-a-translator = A entidade {$entity} não é uma tradutora.
