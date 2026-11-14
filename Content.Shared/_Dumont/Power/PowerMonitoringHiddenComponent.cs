// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Dumont.Power;

/// <summary>
/// Power consumers with this don't show up on power monitoring consoles at all.
/// </summary>
[RegisterComponent]
public sealed partial class PowerMonitoringHiddenComponent : Component;
