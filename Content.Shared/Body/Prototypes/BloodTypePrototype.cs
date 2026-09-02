using Robust.Shared.Prototypes;
using Content.Shared.Damage;

namespace Content.Shared.Body.Prototypes;

[Prototype("bloodtype")]
[DataDefinition]
public sealed partial class BloodTypePrototype : IPrototype
{

    [IdDataField] public string ID { get; private set; } = default!;

    [DataField("Compat")]
    public List<string>? Compatibilities = new();

    [DataField("DamageList")]
    public DamageSpecifier IncompatibilityDamage = new();

}
