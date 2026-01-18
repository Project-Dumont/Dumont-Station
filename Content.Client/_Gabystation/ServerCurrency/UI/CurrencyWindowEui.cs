// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Sara Aldrete's Top Guy <malchanceux@protonmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2026 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2026 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Eui;
using Content.Shared._Gabystation.ServerCurrency.Prototypes;
using Content.Shared.Eui;
using Robust.Shared.Prototypes;
using Content.Shared._Gabystation.ServerCurrency.UI;

namespace Content.Client._Gabystation.ServerCurrency.UI;

public sealed class CurrencyEui : BaseEui
{
    private readonly CurrencyWindow _window;

    public CurrencyEui()
    {
        _window = new CurrencyWindow();
        _window.OnClose += () => SendMessage(new CurrencyEuiMsg.Close());
        _window.TokenStore.OnBuy += OnBuyTokenMsg;
        _window.TitleStore.OnBuy += OnBuyTitleMsg;
        _window.GhostStore.OnBuy += OnBuyGhostMsg;
        _window.OnTitleChanged += OnSelectedTitle;
        _window.OnGhostChanged += OnSelectedGhost;
    }

    private void OnSelectedGhost(ProtoId<GhostSkinListingPrototype>? proto)
    {
        var msg = new CurrencyEuiMsg.SelectGhostSkin()
        {
            ProtoId = proto
        };
        SendMessage(msg);
    }

    private void OnBuyGhostMsg(ProtoId<GhostSkinListingPrototype> ghostId)
    {
        SendMessage(new CurrencyEuiMsg.BuyGhostSkin
        {
            GhostId = ghostId
        });
        SendMessage(new CurrencyEuiMsg.Close());
    }

    private void OnSelectedTitle(ProtoId<TitleListingPrototype>? proto)
    {
        var msg = new CurrencyEuiMsg.SelectTitle()
        {
            ProtoId = proto
        };
        SendMessage(msg);
    }

    private void OnBuyTitleMsg(ProtoId<TitleListingPrototype> titleId)
    {
        SendMessage(new CurrencyEuiMsg.BuyTitle
        {
            TitleId = titleId
        });
        SendMessage(new CurrencyEuiMsg.Close());
    }

    private void OnBuyTokenMsg(ProtoId<TokenListingPrototype> tokenId)
    {
        SendMessage(new CurrencyEuiMsg.BuyToken
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
        _window.UpdateState(s);
    }
}
