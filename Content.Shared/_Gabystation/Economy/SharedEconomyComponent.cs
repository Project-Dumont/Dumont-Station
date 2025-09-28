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
    public required int Balance { get; set; }
    public string? JobId { get; set; }
    public EntityUid? Owner { get; set; }
    public string? OwnerName { get; set; }
}

public interface IBankAccount
{
    int Password { get; set; }
    int InitialPassword { get; set; }
    int Balance { get; set; }
    string? JobId { get; set; }
    EntityUid? Owner { get; set; }
    string? OwnerName { get; set; }
}
