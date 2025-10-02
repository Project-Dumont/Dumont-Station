// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <41803390+Kyoth25f@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later


using Content.Shared._Gabystation.Economy;
using JetBrains.Annotations;

namespace Content.Server._Gabystation.Economy;

/// <summary>
/// When an account receive a payment.
/// </summary>
[PublicAPI]
public sealed class AccountTransferenceCompleted : EntityEventArgs
{
    public TransferenceTypes Type;
    public int AccountId;
    public IBankAccount? Account;
    public EntityUid Uid;
    public int Amount;

    /// <summary>
    /// Used by transference type
    /// </summary>
    public int? TargetAccount;
}

/// <summary>
/// Occurs after all payments.
/// </summary>
[PublicAPI]
public sealed class AfterPaymentRotation : EntityEventArgs
{
    public EntityUid Uid;
}

public enum TransferenceTypes
{
    Payment,
    Transference,
    Purchase,
    Withdraw,
    Deposit,
    Update,
}
