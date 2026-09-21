using Robust.Shared.GameStates;

namespace Content.Shared._Dumont.Coroner;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedCoronerSystem))]
public sealed partial class AutopsyTableComponent : Component;
