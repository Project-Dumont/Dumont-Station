using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server._Gabystation.Economy;

[RegisterComponent, Access(typeof(EconomyManagerSystem))]
public sealed partial class EconomyManagerComponent : Component
{
    /// <summary>
    /// Stores all bank accounts.
    /// </summary>
    [DataField("bankAccounts")]
    public Dictionary<int, IBankAccount> BankAccounts = new Dictionary<int, IBankAccount>();

    /// <summary>
    /// Stores all entities that have an bank account linked.
    /// </summary>
    [DataField("uidBankRef")]
    public Dictionary<EntityUid, int> UidBankRef = new Dictionary<EntityUid, int>();

    [DataField]
    public float PaymentDelay = 60f;

    [DataField]
    public float PaymentCooldownRemaining = 5f;
}
