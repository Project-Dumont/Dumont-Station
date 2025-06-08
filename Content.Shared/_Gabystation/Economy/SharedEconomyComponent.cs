using Robust.Shared.GameStates;

namespace Content.Shared._Gabystation.Economy;

/*[RegisterComponent, NetworkedComponent]
public sealed partial class SharedEconomyManagerComponent : Component
{
}*/

public sealed class BankAccount : IBankAccount
{
    public required int Password { get; set; }
    public required int InitialPassword { get; set; }
    public float Balance { get; set; }
    public required string? JobId { get; set; }
    public required EntityUid? Owner { get; set; }
}

public interface IBankAccount
{
    int Password { get; set; }
    int InitialPassword { get; set; }
    float Balance { get; set; }
    string? JobId { get; set; }
    EntityUid? Owner { get; set; }
}
