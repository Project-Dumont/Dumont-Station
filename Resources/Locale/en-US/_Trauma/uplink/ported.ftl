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

uplink-ammo-20rdmagazine-name = Straight Magazine (.30 rifle)

uplink-ammo-20rdmagazine-desc = A straight 20-round double stack magazine, intended to hold 20 rounds of general-purpose kinetic .30 rifle ammunition.

uplink-cigcarton-name = Interdyne Herbals Carton

uplink-cigcarton-desc = Elite cigarettes for elite agents. Infused with medicine for when you need to do more than calm your nerves. Now sold in cartons!

uplink-storage-briefcase-name = False Bottomed Briefcase

uplink-storage-briefcase-desc = A briefcase with a hidden compartment that can store anything normal-sized or smaller. To use the hidden compartment, use a screwdriver on the briefcase, and then place the desired item inside before closing the compartment by using the screwdriver again. Be warned that anyone with the proper tools can use this briefcase's hidden function or open the compartment!

uplink-codespeak-manual-name = Codespeak Manual

uplink-codespeak-manual-desc = Syndicate agents can be trained to use a series of codewords to convey complex information, which sounds like random concepts and drinks to anyone listening. This manual teaches you this Codespeak. This is the deluxe edition, which has unlimited uses.

uplink-syndicate-access-config-name = Cybersun Configurator

uplink-syndicate-access-config-desc = A Cybersun-branded access configurator, compatible with all non-NanoTrasen IDs. Now with quick-swap functionality!

uplink-viscerators-delivery-grenade-name = Viscerator Delivery Grenade

uplink-viscerators-delivery-grenade-desc = Grenade that deploys a swarm of viscerators upon activation, which will chase down and shred any non-agents in the area.

uplink-cigar-gold-syndicate-name = Explosive Cigar Case

uplink-cigar-gold-syndicate-desc = Capitalize on the Nanotrasen elite’s love for cigars with this certified Waffle Co. classic!* Just light these bad boys up, wait until the first half of the nicotine is used up, then BOOM!                   *Warning! Assassination plots on political enemies may not succeed!

uplink-viscerators-delivery-grenade-box-name = Viscerator Delivery Grenade Box

uplink-viscerators-delivery-grenade-box-desc = A box containing unique grenades that deploys a swarm of viscerators upon activation, which will chase down and shred any non-operatives in the area.

uplink-hyposhell-name = Box of Hyposhells

uplink-hyposhell-desc = A box containing four hyposhells, shotgun shells that can hold 7u of any chemical.

uplink-foxdie-name = FOXDIE Retrovirus

uplink-foxdie-desc = A dna sampler for the legendary FOXDIE retrovirus. Scan something with your target's DNA and it will kill only them, while spreading harmlessly through everyone else.

uplink-snakesfang-name = Snakesfang

uplink-snakesfang-desc = The Snakesfang is a fork-tipped scimitar with a sharp edge and sharper bite. Comes with a scabbard.

uplink-rifle-FAL-name = Saive Automatique

uplink-rifle-FAL-description = The Saive Automatique is an old semi-automatic battle rifle that originates from Terra, it was nicknamed "The right arm of the free world" due to its use in cold war conflicts, it feeds from straight .30 rifle magazines.

uplink-monarch-name = Monarch Coat

uplink-monarch-desc = Directly taken from the wardrobe of Gorlex Entertainment’s most iconic operative, the Monarch Coat is a lightweight armor that uses stolen reflective technology to passively deflect energy blasts… but don't expect too much real protection from a prop.

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

simple-tool-usage-blocked-message = You need a tool that can perform {$quality}!

uplink-knife-rig-name = Throwing knife rig
uplink-knife-rig-desc = A specialized throwing knife rig equipped with 8 Syndicate throwing knives.
