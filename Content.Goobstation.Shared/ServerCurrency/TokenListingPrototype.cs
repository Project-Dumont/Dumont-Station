// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Sara Aldrete's Top Guy <malchanceux@protonmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2026 AgentePanela <agentepanela@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.ServerCurrency;

[Prototype("tokenListing")]
public sealed class TokenListingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    // Gaby change
    [DataField("tokenType", required: true)]
    public string Type { get; private set; } = "Misc";

    [DataField("name", required: true)]
    public string Name { get; private set; } = string.Empty;

    [DataField("price", required: true)]
    public int Price { get; private set; }

    // Gaby change > pass everything bellow to optional
    [DataField("description")]
    public string Description { get; private set; } = "token-generic-desc";

    [DataField("adminNote")]
    public string AdminNote { get; private set; } = "token-generic-note";
}
