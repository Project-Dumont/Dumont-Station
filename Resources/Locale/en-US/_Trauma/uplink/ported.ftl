# SPDX-License-Identifier: AGPL-3.0-or-later

disease-dna-sampler-window-title = DNA Sampler 6000

disease-dna-sampler-no-disease = Retrovirus stock depleted!

disease-dna-sampler-create-injector = Create Injector!

disease-dna-sampler-no-dna = No target DNA sampled, scan an object to begin

disease-dna-sampler-disease = Retrovirus loaded: {$name}

disease-dna-sampler-popup-target = {CAPITALIZE($user)} starts sampling your DNA!

disease-dna-sampler-popup-user = You start sampling DNA from {$target}...

disease-dna-sampler-failed = Failed to sample any DNA from {$target}!

disease-dna-sampler-success = You sampled some DNA from {$target}.

# UI

hijack-beacon-verb-activate-text = Activate

hijack-beacon-verb-activate-message = The beacon can only be armed on the Automated Trade Station, on an unoccupied tile.

hijack-beacon-verb-deactivate-text = Deactivate

hijack-beacon-verb-deactivate-message = The beacon isn't going to deactivate itself, you know.

hijack-beacon-examine-await-activate = The beacon is [color=green]ready to activate[/color].

hijack-beacon-examine-await-cooldown = The beacon is [color=red]on cooldown[/color].

hijack-beacon-examine-await-hijack-complete = The beacon is [color=red]spent[/color].

hijack-beacon-announcement-sender = Automated Trade Station

hijack-beacon-announcement-activated = Attention! An Attempted Breach of the Automated Trade Station's firewall has been detected! Estimated {$time} seconds until firewall breach!

hijack-beacon-announcement-deactivated = Firewall breach failed. Firewall integrity partially restored. Have a nice day!

hijack-beacon-announcement-success = Successfully disengaged Automated Trade Station firewall. {$fine} spesos has been transferred from station funds to [%ERROR%]. Your trade station warranty is now void. This incident has been reported.

hijack-beacon-popup-anchor = The beacon anchors itself into the ground!

hijack-beacon-popup-unanchor = The beacon unanchors itself from the ground.

photograph-name-text-empty = This is a photograph.

photograph-name-text = This is a photograph of { PROPER($entity) ->
    *[false] { INDEFINITE($entity) } { $entity }
     [true] { $entity }
    }.

photograph-name-text-photograph = This is a photograph of another photograph.

solution-refuel-component-on-examine-detailed-message = Fuel: [color={$colorName}]{$fuelLeft}/{$fuelCapacity}[/color]

solution-refuel-component-already-full = {CAPITALIZE(THE($name))} is already full.

solution-refuel-component-no-fuel-in-tank = {CAPITALIZE(THE($target))} has no compatible fuel.

counter-attack-verb = Counter attack

counter-attack-fail-npc-message = They are too unpredictable to counterattack!

counter-attack-self-message = You prepare to counterattack {$target}...

counter-attack-others-message = {$user} widens {POSS-ADJ($user)} stance, {POSS-ADJ($user)} hand hovering over {THE($sheath)}

uplink-travel-camera-name = Travel Camera

uplink-travel-camera-desc = Stun people with your photography skills and the conveniently legal camera flash. Makes you look like a tourist.

uplink-voice-mask-implanter-name = Identity Mask Implanter

uplink-voice-mask-implanter-desc = Modifies your vocal cords and facial structure to be able to mimic anyone you could imagine. Be sure to set your name before speaking, or else you'll give yourself away.

uplink-briefcase-gun-name = Briefcase Gun

uplink-briefcase-gun-desc = An indistinct briefcase with a highly compact C-20K mounted inside it. Careful not the grip the handle too tight!

uplink-hijack-beacon-name = Hijack Beacon

uplink-hijack-beacon-desc = A syndicate-brand hijack beacon designed to get around the firewalls of Nanotrasen-brand Automated Trade Stations. They take 600 seconds to work and Trade Stations will announce they are being hacked, so prepare accordingly.

uplink-adder-name = Adder bundle

uplink-adder-desc = One state-of-the-art Adder plasma pistol, bundled with 2 canisters of its proprietary liquid ammo.

uplink-akurra-name = Akurra bundle

uplink-akurra-desc = Fearsome as it is advanced, the venerable Akurra bundled with 4 canisters of its proprietary liquid ammo.

uplink-flamethrower-name = FG13 Xiuhcoatl flamethrower

uplink-flamethrower-desc = Invite Nanotrasen to the cookout with this portable incendiary weapon.

uplink-leviathan-name = Leviathan bundle

uplink-leviathan-desc = Sometimes, it really is the size that matters.

uplink-magazine-echion-name = Echion canister

uplink-magazine-echion-desc = A special refueling canister containing 75 units of Echion. Compatible with the Adder and Akurra.

uplink-echion-synth-name = Echion synthesizer

uplink-echion-synth-desc = An advanced catalyser that produces Echion at a steady rate while powered. Compatible with the Adder and Akurra.

uplink-rocketexp-name = Leviathan explosive rocket bundle

uplink-rocketexp-desc = A box containing 3 explosive rockets compatible with the Leviathan.

uplink-rocketinc-name = Leviathan incendiary rocket bundle

uplink-rocketinc-desc = A box containing 3 incendiary rockets compatible with the Leviathan.

uplink-rocketcon-name = Leviathan concussion rocket bundle

uplink-rocketcon-desc = A box containing 3 concussion rockets compatible with the Leviathan.

uplink-magic-carp-dehydrated-name = Dehydrated Magic Carp

uplink-magic-carp-dehydrated-desc = Looks like a plush toy carp, but just add water and it becomes a real-life magic carp!

uplink-holo-carp-dehydrated-name = Dehydrated Holo Carp

uplink-holo-carp-dehydrated-desc = Looks like a plush toy carp, but just add water and it becomes a real-life holo carp!

uplink-reinforcement-radio-nukie-carp-name = Carp Operative Teleporter

uplink-reinforcement-radio-nukie-carp-desc = Calls in a specially trained carp operative to assist you.

uplink-reinforcement-radio-nukie-spider-name = Spider Operative Teleporter

uplink-reinforcement-radio-nukie-spider-desc = Calls in a specially trained arachnid operative to assist you.

uplink-reinforcement-radio-cyborg-saboteur-name = Saboteur Cyborg Teleporter

uplink-reinforcement-radio-cyborg-saboteur-desc = Calls in a lightly armored cyborg specializing in sabotage and assassinations.

uplink-reinforcement-radio-cyborg-medical-name = Medical Cyborg Teleporter

uplink-reinforcement-radio-cyborg-medical-desc = Calls in a lightly armored cyborg specializing in anything medical related.

uplink-ammo-20rdmagazine-name = Straight Magazine (.30 rifle)

uplink-ammo-20rdmagazine-desc = A straight 20-round double stack magazine, intended to hold 20 rounds of general-purpose kinetic .30 rifle ammunition.

uplink-caseless-pistol-tranq-name = Pistol Magazine (.25 caseless tranq)

uplink-caseless-pistol-tranq-desc = A magazine with 10 rounds of tranquilizer ammunition loaded with Nocturine and Mute Toxin. Four rapid shots are needed to sedate. For use with the Cobra.

uplink-caseless-pistol-poison-name = Pistol Magazine (.25 caseless toxin)

uplink-caseless-pistol-poison-desc = A magazine with 10 rounds of poisonous ammunition loaded with Lexorin. Takes four shots and a short time to kill. For use with the Cobra.

uplink-cigcarton-name = Interdyne Herbals Carton

uplink-cigcarton-desc = Elite cigarettes for elite agents. Infused with medicine for when you need to do more than calm your nerves. Now sold in cartons!

uplink-records-tablet-name = Records Tablet

uplink-records-tablet-desc = A tablet that lets you view the station records of any crewmember, handily disguised as a station map. One per agent.

uplink-storage-briefcase-name = False Bottomed Briefcase

uplink-storage-briefcase-desc = A briefcase with a hidden compartment that can store anything normal-sized or smaller. To use the hidden compartment, use a screwdriver on the briefcase, and then place the desired item inside before closing the compartment by using the screwdriver again. Be warned that anyone with the proper tools can use this briefcase's hidden function or open the compartment!

uplink-codespeak-manual-name = Codespeak Manual

uplink-codespeak-manual-desc = Syndicate agents can be trained to use a series of codewords to convey complex information, which sounds like random concepts and drinks to anyone listening. This manual teaches you this Codespeak. This is the deluxe edition, which has unlimited uses.

uplink-syndicate-access-config-name = Cybersun Configurator

uplink-syndicate-access-config-desc = A Cybersun-branded access configurator, compatible with all non-NanoTrasen IDs. Now with quick-swap functionality!

uplink-syndicate-remote-name = Syndicate Door Remote

uplink-syndicate-remote-desc = A Syndicate-branded door remote, this one works for Syndicate Agent accesses.

uplink-sticky-grenade-name = Sticky Explosive Grenade

uplink-sticky-grenade-desc = A sticky version of the simplistic grenade with a 3 second long fuse. Causes minimal hull damage.

uplink-viscerators-delivery-grenade-name = Viscerator Delivery Grenade

uplink-viscerators-delivery-grenade-desc = Grenade that deploys a swarm of viscerators upon activation, which will chase down and shred any non-agents in the area.

uplink-cigar-gold-syndicate-name = Explosive Cigar Case

uplink-cigar-gold-syndicate-desc = Capitalize on the Nanotrasen elite’s love for cigars with this certified Waffle Co. classic!* Just light these bad boys up, wait until the first half of the nicotine is used up, then BOOM!                   *Warning! Assassination plots on political enemies may not succeed!

uplink-viscerators-delivery-grenade-box-name = Viscerator Delivery Grenade Box

uplink-viscerators-delivery-grenade-box-desc = A box containing unique grenades that deploys a swarm of viscerators upon activation, which will chase down and shred any non-operatives in the area.

uplink-mimana-stealthy-name = Stealthy Mimana Peel

uplink-mimana-stealthy-desc = A mutated mimana that seems to fade into the background. Don't lose it!

uplink-medipen-mute-name = Mute Toxin Auto Injector

uplink-medipen-mute-desc = An auto injector disguised as an emergency medipen filled with 15 units of mute toxin.

uplink-silent-shoes-name = Silent Shoes

uplink-silent-shoes-desc = A silent threat. No one will hear you coming in this special pair of shoes.

uplink-utility-knife-name = Chemical Utility Knife

uplink-utility-knife-desc = A utility knife with special grooves that can hold up to 10u of any chemical.

uplink-kudzu-grenade-name = Kudzu Grenade

uplink-kudzu-grenade-desc = A grenade containing mutated kudzu that spreads faster and deals damage to anyone caught inside, excluding diona.

uplink-hyposhell-name = Box of Hyposhells

uplink-hyposhell-desc = A box containing four hyposhells, shotgun shells that can hold 7u of any chemical.

uplink-appraisal-tool-jammer-name = Appraisal Tool Radio Jammer

uplink-appraisal-tool-jammer-desc = A radio jammer hidden inside an appraisal tool. You can both jam their comms and price check the corpse!

uplink-foxdie-name = FOXDIE Retrovirus

uplink-foxdie-desc = A dna sampler for the legendary FOXDIE retrovirus. Scan something with your target's DNA and it will kill only them, while spreading harmlessly through everyone else.

uplink-behonker-name = Behonker Operative Teleporter

uplink-behonker-desc = Calls in a specially trained behonker operative to assist you.

uplink-foam-cutlass-name = Sharp Foam Cutlass

uplink-foam-cutlass-desc = A blade disguised as a toy foam cutlass. Perfect for assassins that pretend playing around.

uplink-snakesfang-name = Snakesfang

uplink-snakesfang-desc = The Snakesfang is a fork-tipped scimitar with a sharp edge and sharper bite. Comes with a scabbard.

uplink-rifle-FAL-name = Saive Automatique

uplink-rifle-FAL-description = The Saive Automatique is an old semi-automatic battle rifle that originates from Terra, it was nicknamed "The right arm of the free world" due to its use in cold war conflicts, it feeds from straight .30 rifle magazines.

uplink-slayer-coat-name = S.L.A.Y.E.R Overcoat

uplink-slayer-coat-desc = The S.L.A.Y.E.R. Overcoat has been specially engineered to be worn over the shoulders, and protects its wearer against explosions! Comes with a constant feed to Nanotrasen camera routers and an inbuilt link to the station radio, so stealth is never an option for the operative wearing it!

uplink-bomb-collar-name = Bomb Collar Kit

uplink-bomb-collar-desc = The perfect way to coerce someone into doing you a favour. You know... because of the implication. Pretty useless, unless worn by someone.

uplink-chameleon-armor-name = Chameleon Armored Vest

uplink-chameleon-armor-desc = A chameleon vest that offers moderate protection against most forms of security brutality. Be aware the protection is detectable by thorough examination.

uplink-monarch-name = Monarch Coat

uplink-monarch-desc = Directly taken from the wardrobe of Gorlex Entertainment’s most iconic operative, the Monarch Coat is a lightweight armor that uses stolen reflective technology to passively deflect energy blasts… but don't expect too much real protection from a prop.

uplink-chameleon-eva-suit-name = Chameleon EVA Bundle

uplink-chameleon-eva-suit-desc = Be prepared for any sort of low pressure situation. Can take the appearance of any suit or coat!

ghost-role-information-nukie-mouse-name = Nuclear Operative Mouse

ghost-role-information-nukie-mouse-description = A Nuclear Operative Mouse reinforcement for the Syndicate.

ghost-role-information-nukie-carp-name = Nuclear Operative Carp

ghost-role-information-nukie-carp-description = A Nuclear Operative Carp reinforcement for the Syndicate.

ghost-role-information-nukie-spider-name = Nuclear Operative Spider

ghost-role-information-nukie-spider-description = A Nuclear Operative Spider reinforcement for the Syndicate.

ghost-role-information-syndicate-cyborg-medical-name = Syndicate Medical Cyborg

ghost-role-information-syndicate-behonker-name = Syndicate Behonker

ghost-role-information-syndicate-behonker-desc = A disciple of the Honkmother needs you. Aid them in their time of need.

secret-stash-briefcase = briefcase

store-category-objective = Objective

reagent-name-echion = echion

reagent-desc-echion = A volatile form of exotic matter patented by Cybersun's RnD division. Used as the fuel and ammunition for some of their more advanced weapons.

language-Codespeak-name = Codespeak

language-Codespeak-description = A secret code that sounds like random nonsense to anyone listening.

chat-language-Codespeak-name = Codespeak

item-toggle-examined-on = The light is currently [color=darkgreen]on[/color].

item-toggle-examined-off = The light is currently [color=darkred]off[/color].

foxdie-heart-stops = Your heart stops beating!

codespeak-manual-learned = You learned Syndicate codespeak.

names-mouse-death-commando-dataset-1 = Operator Blue Cheese

names-mouse-death-commando-dataset-2 = Operator AMERICAN CHEESE

names-mouse-death-commando-dataset-3 = Operator Fromage

names-mouse-death-commando-dataset-4 = Operator Asiago

names-mouse-death-commando-dataset-5 = Operator Brie

names-mouse-death-commando-dataset-6 = Operator Camembert

names-mouse-death-commando-dataset-7 = Operator Cheddar

names-mouse-death-commando-dataset-8 = Operator Cotija

names-mouse-death-commando-dataset-9 = Operator Curd

names-mouse-death-commando-dataset-10 = Operator Chevré

names-mouse-death-commando-dataset-11 = Operator Halloumi

names-mouse-death-commando-dataset-12 = Operator Cottage Cheese

names-mouse-death-commando-dataset-13 = Operator Cream Cheese

names-mouse-death-commando-dataset-14 = Operator Swiss Cheese

names-mouse-death-commando-dataset-15 = Operator Wensleydale

names-mouse-death-commando-dataset-16 = Operator Rocamadour

names-mouse-death-commando-dataset-17 = Operator Feta

names-mouse-death-commando-dataset-18 = Operator Mozzarella

names-mouse-death-commando-dataset-19 = Operator Gorgonzola

names-mouse-death-commando-dataset-20 = Operator Grilled Cheese

names-mouse-death-commando-dataset-21 = Operator Gouda

names-mouse-death-commando-dataset-22 = Operator Gruyere

names-mouse-death-commando-dataset-23 = Operator Comté Cheese

names-mouse-death-commando-dataset-24 = Operator Jarlsberg

names-mouse-death-commando-dataset-25 = Operator Reblochon

names-mouse-death-commando-dataset-26 = Operator Raclette

names-mouse-death-commando-dataset-27 = Operator Tomme de Bruyère

names-mouse-death-commando-dataset-28 = Operator Goat Cheese

names-mouse-death-commando-dataset-29 = Operator Manchego

names-mouse-death-commando-dataset-30 = Operator Ewenique

names-mouse-death-commando-dataset-31 = Operator Monterey Jack

names-mouse-death-commando-dataset-32 = Operator Muenster

names-mouse-death-commando-dataset-33 = Operator Parmesan

names-mouse-death-commando-dataset-34 = Operator Pepper Jack

names-mouse-death-commando-dataset-35 = Operator Provolone

names-mouse-death-commando-dataset-36 = Operator String Cheese

names-mouse-death-commando-dataset-37 = Operator La Vache Qui Rit

names-mouse-death-commando-dataset-38 = Operator Mascarpone

names-mouse-death-commando-dataset-39 = Operator Ricotta

names-mouse-death-commando-dataset-40 = Operator Vacherin

names-nukie-carp-dataset-1 = Chomper

names-nukie-carp-dataset-2 = RedFin

names-nukie-carp-dataset-3 = BloodFin

names-nukie-carp-dataset-4 = Riptide

names-nukie-carp-dataset-5 = DepthCharge

names-nukie-carp-dataset-6 = Harpoon

names-nukie-carp-dataset-7 = Torpedo

names-nukie-carp-dataset-8 = ReefBreaker

names-nukie-carp-dataset-9 = HullBreach

names-nukie-carp-dataset-10 = SeaMine

names-nukie-carp-dataset-11 = Kraken

names-nukie-carp-dataset-12 = Leviathan

names-nukie-carp-dataset-13 = Megalodon

names-nukie-carp-dataset-14 = Barracuda

names-nukie-carp-dataset-15 = Piranha

names-nukie-carp-dataset-16 = RazorGill

names-nukie-carp-dataset-17 = IronJaw

names-nukie-carp-dataset-18 = NeedleTooth

names-nukie-carp-dataset-19 = BoneCrusher

names-nukie-carp-dataset-20 = VoidFin

names-nukie-carp-dataset-21 = BlackCurrent

names-nukie-carp-dataset-22 = Maelstrom

names-nukie-carp-dataset-23 = Undertow

names-nukie-carp-dataset-24 = Abyss

names-nukie-carp-dataset-25 = Trench

names-nukie-carp-dataset-26 = ColdWater

names-nukie-carp-dataset-27 = SilentWake

names-nukie-carp-dataset-28 = NightTide

names-nukie-carp-dataset-29 = GhostFin

names-nukie-carp-dataset-30 = BlackReef

names-nukie-carp-dataset-31 = Skipper

names-nukie-carp-dataset-32 = Gnasher

names-nukie-carp-dataset-33 = ScrapMetal

names-nukie-carp-dataset-34 = BioWeapon

names-nukie-carp-dataset-35 = WarFish

names-nukie-carp-dataset-36 = DeathCurrent

names-nukie-carp-dataset-37 = FinFang

names-nukie-carp-dataset-38 = HullHunter

names-nukie-carp-dataset-39 = DeepBite

names-nukie-carp-dataset-40 = Carpocalypse

names-spider-operative-dataset-1 = Zilla

names-spider-operative-dataset-2 = Thrasha

names-spider-operative-dataset-3 = King Gohma

names-spider-operative-dataset-4 = Queen Gohma

names-spider-operative-dataset-5 = MechaZilla

names-spider-operative-dataset-6 = Maneater

names-spider-operative-dataset-7 = OrbWeaver

names-spider-operative-dataset-8 = Archne

names-spider-operative-dataset-9 = Dokugu

names-spider-operative-dataset-10 = Prybar

names-spider-operative-dataset-11 = WebHead

names-spider-operative-dataset-12 = RadiactiveRed

names-spider-operative-dataset-13 = Wolf

names-spider-operative-dataset-14 = WidowMaker

names-spider-operative-dataset-15 = TrapDoor

names-spider-operative-dataset-16 = Spinner

names-spider-operative-dataset-17 = Durathread

names-spider-operative-dataset-18 = WeaverCleaver

names-spider-operative-dataset-19 = NetBall

names-spider-operative-dataset-20 = Tsuchigumo

names-spider-operative-dataset-21 = RedWreckless

names-spider-operative-dataset-22 = Quelaag

names-spider-operative-dataset-23 = ChaosWitch

names-spider-operative-dataset-24 = FireKeeper

names-spider-operative-dataset-25 = SilkSong

names-spider-operative-dataset-26 = PaleHornet

names-spider-operative-dataset-27 = DeepNest

names-spider-operative-dataset-28 = GraveMaker

names-spider-operative-dataset-29 = RedBack

names-spider-operative-dataset-30 = Wanderer

names-spider-operative-dataset-31 = EggSac

names-spider-operative-dataset-32 = FennelWeb

names-spider-operative-dataset-33 = CrewEater

names-spider-operative-dataset-34 = Arachnophobia

names-spider-operative-dataset-35 = RogueWeb

names-spider-operative-dataset-36 = Spectacular

names-spider-operative-dataset-37 = Amazing

names-spider-operative-dataset-38 = Ultimate

names-spider-operative-dataset-39 = Parker

names-spider-operative-dataset-40 = Morales

names-spider-operative-dataset-41 = Stacy

names-spider-operative-dataset-42 = Penny

names-spider-operative-dataset-43 = Noir

names-spider-operative-dataset-44 = Ham

names-spider-operative-dataset-45 = Brown

names-spider-operative-dataset-46 = Hobo

names-nukie-behonker-dataset-1 = Honkmother

names-nukie-behonker-dataset-2 = Bozo

names-nukie-behonker-dataset-3 = Pennywise

names-nukie-behonker-dataset-4 = Cluwne

names-nukie-behonker-dataset-5 = RedNose

names-nukie-behonker-dataset-6 = BigTop

names-nukie-behonker-dataset-7 = Slapstick

names-nukie-behonker-dataset-8 = Whoopee

names-nukie-behonker-dataset-9 = Custardface

names-nukie-behonker-dataset-10 = Buttonz

names-nukie-behonker-dataset-11 = Squeakers

names-nukie-behonker-dataset-12 = Honker

names-nukie-behonker-dataset-13 = Tearjerker

names-nukie-behonker-dataset-14 = Pratfall

names-nukie-behonker-dataset-15 = Sadclown

names-nukie-behonker-dataset-16 = Bananium

names-nukie-behonker-dataset-17 = GreasePaint

names-nukie-behonker-dataset-18 = Facepaint

names-nukie-behonker-dataset-19 = Confetti

names-nukie-behonker-dataset-20 = Streamer

names-nukie-behonker-dataset-21 = Balloonimal

names-nukie-behonker-dataset-22 = Juggler

names-nukie-behonker-dataset-23 = Ringmaster

names-nukie-behonker-dataset-24 = Sideshow

names-nukie-behonker-dataset-25 = Carnie

names-nukie-behonker-dataset-26 = FunHouse

names-nukie-behonker-dataset-27 = HallOfMirrors

names-nukie-behonker-dataset-28 = TentPole

names-nukie-behonker-dataset-29 = Ringleader

names-nukie-behonker-dataset-30 = LaughTrack

names-nukie-behonker-dataset-31 = DarkCarnival

names-nukie-behonker-dataset-32 = Nightmare Fuel

names-nukie-behonker-dataset-33 = ItLives

names-nukie-behonker-dataset-34 = Krusty

names-nukie-behonker-dataset-35 = Chuckles

names-nukie-behonker-dataset-36 = Gigglebyte

names-nukie-behonker-dataset-37 = Wretch

names-nukie-behonker-dataset-38 = FacePunch

names-nukie-behonker-dataset-39 = Deathmask

names-nukie-behonker-dataset-40 = Grimhonk

selectable-lock-verb-category-name = Add lock

selectable-lock-tool-prying-verb = Tool Lock (Crowbar)

selectable-lock-tool-prying-popup = A prying tool lock has been added to {THE($target)}.

selectable-lock-tool-screwing-verb = Tool Lock (Screwdriver)

selectable-lock-tool-screwing-popup = A screwing tool lock has been added to {THE($target)}.

selectable-lock-tool-cutting-verb = Tool Lock (Wirecutter)

selectable-lock-tool-cutting-popup = A cutting tool lock has been added to {THE($target)}.

selectable-lock-voice-verb = Voice Lock

selectable-lock-voice-popup = A voice lock has been added to {THE($target)}.

voice-trigger-lock-verb-record = Record lock phrase

voice-trigger-lock-verb-message = Locking the item will disable features that reveal its true nature!

voice-trigger-lock-on-uninitialized = The display is blank

voice-trigger-lock-on-examine = The display shows the passphrase: "{$keyphrase}"

selectable-lock-no-lock-verb = No lock

selectable-lock-no-lock-popup = No lock has been added to {THE($target)}.

alerts-counter-attacking-name = Counter attacking

alerts-counter-attacking-desc = You are ready to deliver counter attack...

alerts-counter-attacking-blocked-name = Counter attacking blocked

alerts-counter-attacking-blocked-desc = You cannot counter attack for some time...

door-remote-switch-state-toggle-overcharge = Mode: electrify door.

door-remote-toggle-overcharge-text = Electrify

simple-tool-usage-blocked-message = You need a tool that can perform {$quality}!

uplink-knife-rig-name = Throwing knife rig
uplink-knife-rig-desc = A specialized throwing knife rig equipped with 8 Syndicate throwing knives.
