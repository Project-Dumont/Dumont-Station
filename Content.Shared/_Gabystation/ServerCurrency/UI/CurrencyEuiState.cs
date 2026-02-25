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

using Content.Shared._Gabystation.ServerCurrency.Prototypes;
using Content.Shared.Eui;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.ServerCurrency.UI
{
    [Serializable, NetSerializable]
    public sealed class CurrencyEuiState : EuiStateBase
    {
        public float Cooldown;
        public List<string> Tokens = new List<string>();
        public List<ProtoId<TitleListingPrototype>> OwnedTitles = new List<ProtoId<TitleListingPrototype>>();
        public List<ProtoId<GhostSkinListingPrototype>> OwnedGhostSkins = new List<ProtoId<GhostSkinListingPrototype>>();

        public CurrencyEuiState(float cooldown, List<string> tokens, List<ProtoId<TitleListingPrototype>> titles, List<ProtoId<GhostSkinListingPrototype>> ghost)
        {
            Cooldown = cooldown;
            Tokens = tokens;
            OwnedTitles = titles;
            OwnedGhostSkins = ghost;
        }
    }

    public static class CurrencyEuiMsg
    {
        [Serializable, NetSerializable]
        public sealed class Close : EuiMessageBase;

        // Buy messages

        [Serializable, NetSerializable]
        public sealed class BuyToken : EuiMessageBase
        {
            public ProtoId<TokenListingPrototype> TokenId;
        }

        [Serializable, NetSerializable]
        public sealed class BuyTitle : EuiMessageBase
        {
            public ProtoId<TitleListingPrototype> TitleId;
        }

        [Serializable, NetSerializable]
        public sealed class BuyGhostSkin : EuiMessageBase
        {
            public ProtoId<GhostSkinListingPrototype> GhostId;
        }

        // Select msgs

        [Serializable, NetSerializable]
        public sealed class SelectTitle : EuiMessageBase
        {
            public ProtoId<TitleListingPrototype>? ProtoId;
        }
        [Serializable, NetSerializable]
        public sealed class SelectGhostSkin : EuiMessageBase
        {
            public ProtoId<GhostSkinListingPrototype>? ProtoId;
        }
    }
}
