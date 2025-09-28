
using Content.Shared._Gabystation.Economy;
using JetBrains.Annotations;

namespace Content.Server._Gabystation.Economy;

/// <summary>
/// When an account receive an payment.
/// </summary>
[PublicAPI]
public sealed class AccountTransferenceCompleted : EntityEventArgs
{
    public TransferenceTypes Type;
    public IBankAccount? Account;
    public EntityUid Uid;
    public int Amount;

    /// <summary>
    /// Used by transference type
    /// </summary>
    public int? TargetAccount;
}

/// <summary>
/// Occours after all payments.
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
    Pursache,
    Withdraw,
    Deposit
}
