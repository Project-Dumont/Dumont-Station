// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <41803390+Kyoth25f@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Gabystation.Economy;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server._Gabystation.Economy;

[RegisterComponent/*, Access(typeof(EconomyManagerSystem))*/]
public sealed partial class EconomyManagerComponent : Component
{
    /// <summary>
    /// Stores all bank accounts.
    /// </summary>
    [DataField]
    public Dictionary<int, IBankAccount> BankAccounts = new Dictionary<int, IBankAccount>();

    /// <summary>
    /// Stores all entities that have an bank account linked.
    /// </summary>
    [DataField]
    public Dictionary<EntityUid, int> UidBankRef = new Dictionary<EntityUid, int>();

    /// <summary>
    /// Cooldown to the payment time, the default is 300 (5min).
    /// </summary>
    [DataField]
    public float PaymentCooldownRemaining = 300;

    [DataField]
    public float PaymentDelay = 300;
}


