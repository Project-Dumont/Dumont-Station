using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NanoBankUiState : BoundUserInterfaceState
{
    public readonly int AccountId;
    public readonly int Pin;
    public readonly bool NotificationsMuted;
    public readonly bool Logged;
    public readonly float NextPayment;
    public readonly float Balance;

    public NanoBankUiState(
        int accountId,
        int pin,
        bool notificationsMuted,
        bool logged,
        float nextPayment,
        float balance)
    {
        AccountId = accountId;
        Pin = pin;
        NotificationsMuted = notificationsMuted;
        Logged = logged;
        NextPayment = nextPayment;
        Balance = balance;
    }
}
