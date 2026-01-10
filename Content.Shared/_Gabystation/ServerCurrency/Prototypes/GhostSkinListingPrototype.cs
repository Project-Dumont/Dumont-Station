// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Gabystation.ServerCurrency.Prototypes;

[Prototype("ghostSkinListing")]
public sealed class GhostSkinListingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;


    [DataField("name", required: true)]
    public string Name { get; private set; } = string.Empty;

    [DataField("price", required: true)]
    public int Price { get; private set; } = 0;

    [DataField("sprite", required: true)]
    public SpriteSpecifier? Sprite { get; private set; }

    [DataField("avaible", required: false)]
    public bool Avaible { get; private set; } = true; //todo this & comand giveTitle
}
