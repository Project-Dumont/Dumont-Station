// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Gabystation.ATM;

[RegisterComponent, NetworkedComponent]
public sealed partial class BankATMComponent : Component
{
    public static string CashSlotId = "bankATM-cashSlot";

    [DataField]
    public ItemSlot CashSlot = new();

    public static string CardSlotId = "bankATM-cardSlot";

    [DataField]
    public ItemSlot CardSlot = new();

    [DataField("cashPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>)),
        ViewVariables(VVAccess.ReadWrite)]
    public string CashPrototype = "SpaceCash";

    [DataField]
    public SoundSpecifier PrintSound = new SoundPathSpecifier("/Audio/Machines/printer.ogg");
    [DataField]
    public SoundSpecifier DepositSound = new SoundPathSpecifier("/Audio/Effects/Cargo/beep.ogg");
    public float Printing = 0f;
    public int MoneyToPrint = 0;
}

[Serializable, NetSerializable]
public sealed class BankATMBuiState : BoundUserInterfaceState
{
    public int Balance;
    public bool HasCard;
    public int? AccountId;
    public int? MoneyInside;
    public string? Message;
}

[Serializable, NetSerializable]
public enum BankATMUIKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class BankATMMessage : BoundUserInterfaceMessage
{
    public BankATMMsgType Type;
    public int? Amount;
}
public enum BankATMMsgType
{
    Withdraw,
    Deposit
}
