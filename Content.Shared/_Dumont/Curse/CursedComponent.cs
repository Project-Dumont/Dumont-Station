using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Dumont.Curse;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CursedComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<MasonCursePrototype>> Curses = new();
}
