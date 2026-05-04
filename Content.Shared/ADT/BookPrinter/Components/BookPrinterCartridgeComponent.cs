// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Оригинал данного файла был сделан @temporaldarkness (discord). Код был взят с https://github.com/ss14-ganimed/ENT14-Master.

using Robust.Shared.GameStates;

namespace Content.Shared.ADT.BookPrinter.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BookPrinterCartridgeComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float FullCharge = 20.0f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float CurrentCharge = 20.0f;
}
