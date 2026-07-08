// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mutatrix.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Mutatrix.Components;

/// <summary>
/// Runtime marker added to a mob while a Mutatrix is equipped and controlling it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedMutatrixSystem))]
public sealed partial class ActiveMutatrixComponent : Component
{
    /// <summary>
    /// The equipped Mutatrix item.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid Device;
}
