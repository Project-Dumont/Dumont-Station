### UI

# Displayed in the Character prefs window
# Não da pra usar as funções auxiliares pois $gender é uma string de gênero, e não uma entidade com o componente Grammar.
humanoid-character-profile-summary = { $gender ->
    *[male] Esse é {$name}. Ele
    [female] Essa é {$name}. Ela
    [other] Essu é {$name}. Elu
} tem {$age} anos.