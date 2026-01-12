// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Sara Aldrete's Top Guy <malchanceux@protonmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2026 AgentePanela <agentepanela@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Shared.ServerCurrency;
using Content.Goobstation.Shared.ServerCurrency.UI;
using Content.Server._Gabystation.ServerCurrency;
using Content.Server._Gabystation.ServerCurrency.Managers;
using Content.Shared._Gabystation.ServerCurrency.Prototypes;
using Content.Server.Administration.Notes;
using Content.Server.EUI;
using Content.Shared.Eui;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.ServerCurrency.UI
{
    public sealed class CurrencyEui : BaseEui
    {
        [Dependency] private readonly ICommonCurrencyManager _currencyMan = default!;
        [Dependency] private readonly IAdminNotesManager _notesMan = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IEntitySystemManager _entMan = default!;
        [Dependency] private readonly CurrencyStoreManager _storeMan = default!;
        private readonly ServerCurrencyStoreSystem _store;

        public CurrencyEui()
        {
            IoCManager.InjectDependencies(this);
            _store = _entMan.GetEntitySystem<ServerCurrencyStoreSystem>();
            _store.NewRotation += () => StateDirty();
        }

        public override void Opened()
        {
            StateDirty();
        }

        public override EuiStateBase GetNewState()
        {
            List<string> tokens = [];
            foreach (var token in _store.RotationStorage)
            {
                tokens.Add(token.ID);
            }
            var titles = _storeMan.GetOwnedTitles(Player.UserId);
            return new CurrencyEuiState(_store.RotationCooldown, tokens, titles);
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);
            switch (msg)
            {
                case CurrencyEuiMsg.SelectTitle sel:
                    if (sel.TitleId == "Default")
                        sel.TitleId = null;

                    _storeMan.TrySetTitle(Player.UserId, sel.TitleId);
                    break;

                case CurrencyEuiMsg.BuyToken buy:
                    BuyToken(buy.TokenId, Player);
                    StateDirty();
                    break;

                case CurrencyEuiMsg.BuyTitle buy:
                    BuyTitle(buy.TitleId);
                    StateDirty();
                    break;
            }
        }

        private void BuyTitle(ProtoId<TitleListingPrototype> titleId)
        {
            if (!_protoMan.TryIndex<TitleListingPrototype>(titleId, out var title))
                return;

            if (!_currencyMan.CanAfford(Player.UserId, title.Price, out _))
                return;

            if (!title.Avaible || _storeMan.HasTitle(Player.UserId, titleId))
                return;

            _currencyMan.RemoveCurrency(Player.UserId, title.Price);
            _storeMan.AddTitle(Player.UserId, titleId);
        }

        private async void BuyToken(ProtoId<TokenListingPrototype> tokenId, ICommonSession playerName)
        {
            var balance = _currencyMan.GetBalance(Player.UserId);

            if (!_protoMan.TryIndex<TokenListingPrototype>(tokenId, out var token))
                return;

            if (!_store.RotationStorage.Contains(token))
                return;

            if (balance < token.Price)
                return;

            // This looks fucked up but i wanted to make the token prototype less verbose
            var remark = "Something went wrong - please refund " + token.Price;
            if (token.Type == "Antag")
                remark = Loc.GetString("gs-balanceui-shop-token-antag-remark", ("token", Loc.GetString(token.Name)));
            if (token.Type == "GhostRole")
                remark = Loc.GetString("gs-balanceui-shop-token-ghost-role-remark", ("token", Loc.GetString(token.Name)));
            else
                remark = Loc.GetString(token.AdminNote);

            await _notesMan.AddAdminRemark(Player, Player.UserId, 0,
                Loc.GetString(remark), 0, false, null);
            _currencyMan.RemoveCurrency(Player.UserId, token.Price);
        }
    }
}
