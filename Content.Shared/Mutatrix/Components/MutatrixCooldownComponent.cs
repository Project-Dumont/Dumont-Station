// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mutatrix.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Mutatrix.Components;

/// <summary>
/// Runtime cooldown applied after leaving a Mutatrix transformation.
/// While present and not expired, transformation and scan actions are blocked.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedMutatrixSystem))]
public sealed partial class MutatrixCooldownComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan EndTime;
}
