using Robust.Shared.GameStates;

namespace Content.Shared.Genetics;

[RegisterComponent, NetworkedComponent]
public sealed partial class DnaModifierConsoleComponent : Component
{
    public const string ScannerPort = "MedicalScannerSender";

    [ViewVariables]
    public EntityUid? GeneticScanner = null;

    [DataField("maxDistance")]
    public float MaxDistance = 4f;

    public bool GeneticScannerInRange = true;

    public TimeSpan NextUpdate;
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastInjectorTime;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastSubjectInjectTime;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan InjectorCooldown = TimeSpan.FromMinutes(2);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan SubjectInjectCooldown = TimeSpan.FromMinutes(2);
}
