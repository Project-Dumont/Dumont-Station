// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Mutatrix.GreyMatter.Components;

/// <summary>
/// Marker/config component for the Mutatrix "Massa Cinzenta" form.
/// This form is intentionally tiny and fragile, but excellent at interacting with machines.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MutatrixGreyMatterComponent : Component
{
    /// <summary>
    /// Max distance, in tiles, for machine manipulation powers.
    /// </summary>
    [DataField]
    public float ManipulationRange = 2.0f;
}
