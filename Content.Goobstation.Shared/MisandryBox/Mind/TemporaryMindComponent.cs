// SPDX-FileCopyrightText: 2026 Goob Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.MisandryBox.Mind;

/// <summary>
/// Used in cases the mind has to be somewhere else without interacting the main mind or inheriting what it knows.
/// </summary>
[RegisterComponent]
public sealed partial class TemporaryMindComponent : Component
{
    [DataField]
    public EntityUid OriginalMind;

    [DataField]
    public EntityUid DisposableMind;
}
