// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Gabystation.ServerCurrency.Prototypes;

[Prototype("titleListing")]
public sealed class TitleListingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;


    [DataField("title", required: true)]
    public string Title { get; private set; } = string.Empty;

    [DataField("price", required: true)]
    public int Price { get; private set; } = 0;

    [DataField("color", required: false)]
    public string Color { get; private set; } = string.Empty;

    [DataField("avaible", required: false)]
    public bool Avaible { get; private set; } = true; //todo this & comand giveTitle
}
