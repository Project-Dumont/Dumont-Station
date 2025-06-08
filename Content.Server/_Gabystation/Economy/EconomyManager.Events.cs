
using Content.Shared._Gabystation.Economy;
using JetBrains.Annotations;

namespace Content.Server._Gabystation.Economy;

/// <summary>
/// When an account receive an payment.
/// </summary>
[PublicAPI]
public sealed class AccountPaymentCompleted : EntityEventArgs
{
    public int AccountId;
    public IBankAccount? Account;
    public EntityUid Uid;
    public float Payment;
}

/// <summary>
/// Occours after all payments.
/// </summary>
[PublicAPI]
public sealed class AfterPaymentRotation : EntityEventArgs
{
    public EntityUid Uid;
}
