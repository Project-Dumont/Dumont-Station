# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

server-currency-name-singular = Goob Coin
server-currency-name-plural = Goob Coins

## Commands

server-currency-gift-command = gift
server-currency-gift-command-description = Gifts some of your balance to another player.
server-currency-gift-command-help = Usage: gift <player> <value>
server-currency-gift-command-error-1 = You can't gift yourself!
server-currency-gift-command-error-2 = You can not afford to gift this! You have a balance of {$balance}.
server-currency-gift-command-giver = You gave {$player} {$amount}.
server-currency-gift-command-reciever = {$player} gave you {$amount}.

server-currency-balance-command = balance
server-currency-balance-command-description = Returns your balance.
server-currency-balance-command-help = Usage: balance
server-currency-balance-command-return = You have {$balance}.

server-currency-add-command = balance:add
server-currency-add-command-description = Adds currency to a player's balance.
server-currency-add-command-help = Usage: balance:add <player> <value>

server-currency-remove-command = balance:rem
server-currency-remove-command-description = Removes currency from a player's balance.
server-currency-remove-command-help = Usage: balance:rem <player> <value>

server-currency-set-command = balance:set
server-currency-set-command-description = Sets a player's balance.
server-currency-set-command-help = Usage: balance:set <player> <value>

server-currency-get-command = balance:get
server-currency-get-command-description = Gets the balance of a player.
server-currency-get-command-help = Usage: balance:get <player>

server-currency-command-completion-1 = Username
server-currency-command-completion-2 = Value
server-currency-command-error-1 = Unable to find a player by that name.
server-currency-command-error-2 = Value must be an integer.
server-currency-command-return = {$player} has {$balance}.

# 65% Update

gs-balanceui-title = Store
gs-balanceui-confirm = Confirm

gs-balanceui-gift-label = Transfer:
gs-balanceui-gift-player = Player
gs-balanceui-gift-player-tooltip = Enter the name of the player you want to give Gabycoins to.
gs-balanceui-gift-value = Amount
gs-balanceui-gift-value-tooltip = Amount of Gabycoins to transfer

gs-balanceui-select-title = Select OOC title:
gs-balanceui-title-default = None

gs-balanceui-select-ghost = Select ghost skin:
gs-balanceui-ghost-skin-default = Default

gs-balanceui-shop-label = Token Store
gs-balanceui-shop-empty = Out of stock!
gs-balanceui-shop-buy = Buy
gs-balanceui-shop-cooldown = Next rotation in: {$cooldown}
gs-balanceui-shop-rotation-desc = The store is restocked with
                            {""} {$tokens} tokens every {$cooldown} minutes.
gs-balanceui-shop-footer = ⚠ Use AHelp to redeem your token.
gs-balanceui-shop-buy-btn = Buy {$token} - {$price}gc

gs-balanceui-shop-token-label = Tokens
gs-balanceui-shop-tittle-label = Title

gs-balanceui-shop-token-antag-buy = {$token} Token
gs-balanceui-shop-token-antag-desc = Allows you to become a {$token} while alive in the round.
gs-balanceui-shop-token-antag-remark = Bought an antag token of "{$token}" - Transform while alive in the round.

gs-balanceui-token-traitor = Traitor
gs-balanceui-token-zombie = Zombie
gs-balanceui-token-thief = Thief
gs-balanceui-token-paradox-clone = Paradox Clone
gs-balanceui-token-heretic = Heretic
gs-balanceui-token-wizard = Wizard
gs-balanceui-token-changeling = Changeling
gs-balanceui-token-blob = Blob
gs-balanceui-token-devil = Devil
gs-balanceui-token-shadowling = Shadowling

gs-balanceui-shop-token-antag = Round Antagonist Token
gs-balanceui-shop-token-ghost = Ghost Role Token
gs-balanceui-shop-token-admin-rp = Roleplaying Token
gs-balanceui-shop-token-hat = Hat Token
gs-balanceui-shop-token-cloth = Clothing Token

gs-balanceui-shop-buy-token-antag-desc = Allows you to become the round antagonist.
gs-balanceui-shop-buy-token-ghost-desc = Allows you to request a ghost role to spawn. (antagonists are only valid after 40 minutes into the round)
gs-balanceui-shop-buy-token-admin-rp-desc = Allows you to ask an admin for help to assist with a Roleplay you want, so the RP has the effect desired by the player. The token must not be used to gain competitive advantage (PVP, objectives...).
gs-balanceui-shop-buy-token-hat-desc = Ask an admin for a cosmetic hat.
gs-balanceui-shop-buy-token-cloth-desc = Ask an admin for any allowed cosmetic clothing item.

gs-balanceui-admin-add-label = Add (or subtract) Gabycoins:
gs-balanceui-admin-add-player = Player Name
gs-balanceui-admin-add-value = Amount

gs-balanceui-remark-token-antag = Bought an antag token valid for the round antagonist.
gs-balanceui-remark-token-ghost = Bought a ghost role token. (antagonists are only valid after 40 minutes into the round)
gs-balanceui-remark-token-admin-rp = Bought an admin RP token. (low tier)
gs-balanceui-remark-token-hat = Bought a hat token. (Cosmetic hats)
gs-balanceui-remark-token-cloth = Bought a clothing token. (Cosmetic clothing) | (1 per item)

gs-balanceui-shop-click-confirm = Press again to confirm
gs-balanceui-shop-purchased = Purchased {$item}

