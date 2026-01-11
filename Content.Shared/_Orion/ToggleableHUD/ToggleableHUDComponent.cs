using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Orion.ToggleableHUD;

/// <summary>
///     Toggleable HUD component.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ToggleableHUDComponent : Component
{
    [DataField]
    public EntProtoId Action = "ActionToggleHUDs";

    [DataField]
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField]
    public bool IsToggled;

    [DataField]
    public List<ProtoId<DamageContainerPrototype>> DamageContainers = new()
    {
        "Biological",
        "Inorganic",
        "Silicon",
        "SiliconRadiation",
    };

    [DataField]
    public string PopupToggleOn = "ghost-gui-toggle-hud-popup-on";

    [DataField]
    public string PopupToggleOff = "ghost-gui-toggle-hud-popup-off";
}
