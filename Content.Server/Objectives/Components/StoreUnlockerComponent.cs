// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-License-Identifier: MIT

using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Unlocks store listings that use <see cref="ObjectiveUnlockCondition"/>.
/// </summary>
[RegisterComponent]
public sealed partial class StoreUnlockerComponent : Component
{
    [DataField(required: true)]
    public List<ProtoId<ListingPrototype>> Listings = new();
}
