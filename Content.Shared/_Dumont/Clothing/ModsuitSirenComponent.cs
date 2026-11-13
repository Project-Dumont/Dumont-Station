using Content.Shared.Actions;
using Robust.Shared.GameObjects;

namespace Content.Shared._Dumont.Clothing;

/// <summary>
/// Provides the paramedic modsuit helmet's siren while equipped with a powered suit.
/// Runtime action and emitter state are managed on the server.
/// </summary>
[RegisterComponent]
public sealed partial class ModsuitSirenComponent : Component
{
    public EntityUid? Action;
    public EntityUid? Emitter;
}

/// <summary>
/// Toggles the siren independently of the helmet's handheld-light action.
/// </summary>
public sealed partial class ModsuitSirenToggleEvent : InstantActionEvent;
