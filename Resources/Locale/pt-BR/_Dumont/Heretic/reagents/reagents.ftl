reagent-name-eldritch = essência sobrenatural
reagent-desc-eldritch = Um líquido estranho que desafia as leis da física. Revigora e cura aqueles que enxergam além desta realidade frágil, mas é extremamente nocivo às mentes fechadas.
reagent-name-crucible-soul = alma do cadinho
reagent-desc-crucible-soul = Um líquido translúcido de cor laranja viva. Permite atravessar paredes. Quando o efeito acaba, você volta ao local de origem.
reagent-name-clarity = ocaso e aurora
reagent-desc-clarity = Um líquido amarelo opaco que parece desaparecer e reaparecer em intervalos regulares. Permite enxergar através de paredes e objetos.
reagent-name-marshal = soldado ferido
reagent-desc-marshal = Um líquido escuro e sem cor. Aumenta sua força física, tornando os ataques mais ferozes e reduzindo o dano recebido conforme seus ferimentos aumentam. Seus ataques corpo a corpo recuperam saúde e vigor, mas você sofre dano contínuo. Quanto melhor estiver sua saúde, maior será esse dano.
reagent-name-ether = éter do recém-nascido
reagent-desc-ether = Um líquido verde e espesso que provoca náusea. Restaura completamente seu corpo e depois induz um sono profundo por um minuto inteiro.
reagent-name-eldritch-rust = ferrugem sobrenatural
reagent-desc-eldritch-rust = Uma mistura marrom, viscosa e cheia de grumos.

entity-condition-guidebook-heretic-or-ghoul = o alvo é um herege ou carniçal
entity-condition-guidebook-not-heretic-or-ghoul = o alvo não é um herege nem um carniçal
entity-condition-guidebook-environment-temperature = a temperatura do ambiente é de
    { $invert ->
        [true] pelo menos
        *[false] no máximo
    } { $threshold } graus
entity-condition-guidebook-has-body-part = o alvo
    { $invert ->
        [true] não possui
        *[false] possui
    } { $part }
entity-condition-guidebook-on-fire = o alvo
    { $invert ->
        [true] não está em chamas
        *[false] está em chamas
    }
reagent-effect-guidebook-has-status-effect =
    { $invert ->
        [true] não possui
        *[false] possui
    } o efeito { $effect }
entity-condition-guidebook-nullrod-protected = o alvo está protegido por um bastão nulo
entity-condition-guidebook-nullrod-not-protected = o alvo não está protegido por um bastão nulo
reagent-effect-guidebook-deconvert-ghoul = desfaz a transformação em carniçal
reagent-physical-desc-eldritch = sobrenatural
reagent-physical-desc-crucible-soul = de outro mundo
reagent-physical-desc-clarity = límpido
reagent-physical-desc-marshal = agonizante
reagent-physical-desc-ether = entorpecente
reagent-physical-desc-rusty = enferrujado
flavor-complex-eldritch = Ag'hsj'saje'sh
flavor-complex-crucible-soul = como algo entre os planos
flavor-complex-clarity = como olhos
flavor-complex-marshal = doloroso
flavor-complex-ether = refrescante
flavor-complex-rust = como cobre apodrecido
crucible-soul-effect-examine-message = [color=#fb793a]{ CAPITALIZE(SUBJECT($ent)) } parece estar apenas parcialmente presente.[/color]
wounded-solider-effect-examine-message = [color=#5e718e]{ CAPITALIZE(SUBJECT($ent)) } está em um frenesi imortal.[/color]
