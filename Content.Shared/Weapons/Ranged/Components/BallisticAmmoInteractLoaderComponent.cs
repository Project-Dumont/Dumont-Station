// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// If an entity with <see cref="BallisticAmmoProviderComponent"/> has this component, it can be used on the ammo
/// entity to load it into the gun, instead of the other way around.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BallisticAmmoInteractLoaderComponent : Component;
