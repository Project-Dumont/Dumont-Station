using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.WashingMachine;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class WashingMachineComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan WashingTime;

    [ViewVariables, AutoNetworkedField, AutoPausedField, Access(typeof(SharedWashingMachineSystem))]
    public TimeSpan WashingFinished;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? WashingSound;

    public EntityUid? WashingSoundStream;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? FinishedSound;

    [ViewVariables, AutoNetworkedField, Access(typeof(SharedWashingMachineSystem))]
    public WashingMachineState WashingMachineState;
}
