// SPDX-FileCopyrightText: 2025 CrimeMoot <wakeafa@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;

namespace Content.Shared._Ganimed.SeedDna.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SeedDnaConsoleComponent : Component
{
    public static string SeedSlotId = "SeedSlotId";
    public static string DnaDiskSlotId = "DnaDiskSlotId";

    [DataField]
    public ItemSlot SeedSlot = new();

    [DataField]
    public ItemSlot DnaDiskSlot = new();
}
