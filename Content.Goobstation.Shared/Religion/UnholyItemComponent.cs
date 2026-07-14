// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Religion;

/// <summary>
/// Marks an item as unholy.
///
/// When <see cref="Punish"/> is enabled, unauthorized entities that attempt
/// to pick up or pull this item are punished by <see cref="UnholyItemSystem"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class UnholyItemComponent : Component
{
    /// <summary>
    /// Whether interacting with this item should punish entities that are
    /// neither unholy nor capable of using bibles.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Punish = false;
}
