// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Shared.ServerCurrency;
using Content.Goobstation.Shared.ServerCurrency.UI;
using Content.Server._Gabystation.ServerCurrency;
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
        private readonly ServerCurrencyStoreSystem _store;
        //[Dependency] private readonly ServerCurrencyStoreSystem _store = default!;
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
            return new CurrencyEuiState(_store.RotationCooldown, _store.RotationStorage);
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);
            switch (msg)
            {
                case CurrencyEuiMsg.Buy buy:

                    BuyToken(buy.TokenId, Player);
                    StateDirty();
                    break; //grrr fix formatting
            }
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

            var remark = "Something went wrong - please refund " + token.Price;
            if (token.Type == "Antag")
                remark = Loc.GetString("gs-balanceui-shop-token-antag-remark", ("token", Loc.GetString(token.Name)));
            else
                remark = Loc.GetString(token.AdminNote);

            await _notesMan.AddAdminRemark(Player, Player.UserId, 0,
                Loc.GetString(remark), 0, false, null);
            _currencyMan.RemoveCurrency(Player.UserId, token.Price);
        }
    }
}
