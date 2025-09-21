# templates
# service
ntr-document-service-starting-text1 = [color=#009100]█▄ █ ▀█▀    [head=3]Documento NanoTrasen[/head]
    █ ▀█     █        Para: Departamento de Serviço
                           De: CentComm
                           Emitido: {$date}
    ──────────────────────────────────────────[/color]

# security
ntr-document-security-starting-text1 = [head=3]Documento NanoTrasen[/head]                               [color=#990909]█▄ █ ▀█▀
    Para: Departamento de Segurança                                       █ ▀█     █
    De: CentComm
    Emitido: {$date}
    ──────────────────────────────────────────[/color]

# cargo
ntr-document-cargo-starting-text1 = [head=3]  NanoTrasen[/head]        [color=#d48311]█▄ █ ▀█▀ [/color][bold]      Para: Departamento de Carga[/bold][head=3]
         Documento[/head]           [color=#d48311]█ ▀█     █       [/color] [bold]   De: CentComm[/bold]
     ──────────────────────────────────────────
                                                     Emitido: {$date}

# medical
ntr-document-medical-starting-text1 = [color=#118fd4]░             █▄ █ ▀█▀    [head=3]Documento NanoTrasen[/head]                 ░
    █             █ ▀█     █        Para: Departamento Médico                         █
    ░                                    De: CentComm                                     ░
                                         Emitido: {$date}
    ──────────────────────────────────────────[/color]

# engineering
ntr-document-engineering-starting-text1 = [color=#a15000]█▄ █ ▀█▀    [head=3]Documento NanoTrasen[/head]
    █ ▀█     █        Para: Departamento de Engenharia
                           De: CentComm
                           Emitido: {$date}
    ──────────────────────────────────────────[/color]

# science
ntr-document-science-starting-text1 = [color=#94196f]░             █▄ █ ▀█▀    [head=3]Documento NanoTrasen[/head]                 ░
    █             █ ▀█     █        Para: Departamento de Ciência                         █
    ░                                    De: CentComm                                     ░
                                         Emitido: {$date}
    ──────────────────────────────────────────[/color]
ntr-document-service-document-text =
    {$start}
    A corporação quer que você saiba que você não é {$text1} {$text2}
    A corporação ficaria satisfeita se você {$text3}
    Os carimbos abaixo confirmam que {$text4}

ntr-document-security-document-text =
    {$start}
    A corporação quer que você verifique algumas coisas antes de carimbar este documento, certifique-se de que {$text1} {$text2}
    {$text3}
    {$text4}

ntr-document-cargo-document-text =
    {$start}
    {$text1}
    {$text2}
    Ao carimbar aqui, você {$text3}

ntr-document-medical-document-text =
    {$start}
    {$text1} {$text2}
    {$text3}
    Ao carimbar aqui, você {$text4}

ntr-document-engineering-document-text =
    {$start}
    {$text1} {$text2}
    {$text3}
    Ao carimbar aqui, você {$text4}

ntr-document-science-document-text =
    {$start}
    Estamos monitorando de perto o Departamento de Pesquisa. {$text1} {$text2}
    devido a tudo acima, queremos que você garanta {$text3}
    os carimbos abaixo confirmam {$text4}
