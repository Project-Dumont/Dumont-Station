using Robust.Shared.GameStates;

namespace Content.Shared._Gabystation.SurgeryBlocker;

/// <summary>
/// Impede que alguem tente usar cirurgia em alguma parte espescifica 
/// Usei no aracne porque o EE é maluco
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryBlockerComponent : Component
{
}
