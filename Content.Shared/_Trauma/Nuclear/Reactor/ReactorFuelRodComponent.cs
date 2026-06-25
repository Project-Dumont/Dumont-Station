// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Trauma.Nuclear.Reactor;

/// <summary>
/// A reactor fuel rod which generates heat and neutrons when irradiated.
/// Once fully spent it can be recycled in a centrifuge.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ReactorFuelRodComponent : Component;
