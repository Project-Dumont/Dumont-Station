// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Trauma.Nuclear.Monitor;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentPause]
public sealed partial class NuclearReactorMonitorComponent : Component
{
    [AutoPausedField]
    public TimeSpan NextUpdate;

    /// <summary>
    /// How long to wait between each UI update.
    /// </summary>
    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(0.5);
}
