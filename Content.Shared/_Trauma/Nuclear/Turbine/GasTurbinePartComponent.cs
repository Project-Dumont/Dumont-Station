// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Trauma.Nuclear.Turbine;

[RegisterComponent, NetworkedComponent]
public sealed partial class GasTurbineBladeComponent : Component;

[RegisterComponent, NetworkedComponent]
public sealed partial class GasTurbineStatorComponent : Component;
