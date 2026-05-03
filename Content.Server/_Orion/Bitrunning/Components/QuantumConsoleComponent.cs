using Content.Server._Orion.Bitrunning.Systems;

namespace Content.Server._Orion.Bitrunning.Components;

[RegisterComponent]
public sealed partial class QuantumConsoleComponent : Component
{
    [Access(typeof(QuantumConsoleSystem))]
    public EntityUid? LinkedServerId;
}
