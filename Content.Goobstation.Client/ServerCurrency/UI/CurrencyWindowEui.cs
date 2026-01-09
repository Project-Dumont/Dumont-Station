// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Eui;
using Content.Goobstation.Shared.ServerCurrency;
using Content.Goobstation.Shared.ServerCurrency.UI;
using Content.Shared.Eui;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.ServerCurrency.UI
{
    public sealed class CurrencyEui : BaseEui
    {
        private readonly CurrencyWindow _window;
        public CurrencyEui()
        {
            _window = new CurrencyWindow();
            _window.OnClose += () => SendMessage(new CurrencyEuiMsg.Close());
            _window.OnBuy += OnBuyMsg;
        }

        private void OnBuyMsg(ProtoId<TokenListingPrototype> tokenId)
        {
            SendMessage(new CurrencyEuiMsg.Buy
            {
                TokenId = tokenId
            });
            SendMessage(new CurrencyEuiMsg.Close());
        }

        public override void Opened()
        {
            _window.OpenCentered();
        }
        public override void Closed()
        {
            _window.Close();
        }

        public override void HandleState(EuiStateBase state)
        {
            base.HandleState(state);
            if (state is not CurrencyEuiState s)
                return;

            _window.Cooldown = s.Cooldown;
            _window.UpdateState(s);
        }
    }
}
