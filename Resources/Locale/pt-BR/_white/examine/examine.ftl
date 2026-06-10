# Poggers examine system

examine-name = Isso é [bold]{$name}[/bold]!
examine-name-selfaware = Isso é você!
examine-can-see = Ao observar  { PRONOME-ELE($ent) }, você vê:
examine-can-see-nothing = { CAPITALIZE(PRONOME-ELE($ent)) } está completamente nu!
examine-border-line = ═════════════════════
examine-present-tex = Isto é [enttex id="{ $id }" size={ $size }] [bold]{$name}[/bold]!
examine-present = Isto é [bold]{$name}[/bold]!
examine-present-line = ═══

id-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} no ID.
head-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} na cabeça.
eyes-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} nos olhos.
mask-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} no rosto.
neck-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} no pescoço.
ears-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} nas orelhas.
jumpsuit-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} nas vestimentas.
outer-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} no corpo.
suitstorage-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} no ombro.
back-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} nas costas.
gloves-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} nas mãos.
belt-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} no cinto.
shoes-examine = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} nos pés.

id-card-examine-full = • {CAPITALIZE(POSS-ADJ($wearer))} ID: [bold]{$nameAndJob}[/bold].

# Selfaware version

examine-can-see-selfaware = Ao se olhar, você vê:
examine-can-see-nothing-selfaware = Você está completamente nu!


id-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} no seu ID.
head-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} na sua cabeça.
eyes-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} em seus olhos.
mask-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} no seu rosto.
neck-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} no seu pescoço.
ears-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} em suas orelhas.
jumpsuit-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} em suas vestimentas.
outer-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} em seu corpo.
suitstorage-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} em seu ombro.
back-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} nas suas costas.
gloves-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} em suas mãos.
belt-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} no seu cinto.
shoes-examine-selfaware = • { $id ->
     [empty] [bold]{$item}[/bold]
    *[other] [enttex id="{ $id }" size={ $size }][bold]{$item}[/bold]
} em seus pés.

# Selfaware examine

comp-hands-examine-empty-selfaware = Você não está segurando nada.
comp-hands-examine-selfaware = Você está segurando { $items }.

humanoid-appearance-component-examine-selfaware = Você é { INDEFINITE($age) } { $age } { $species }.
