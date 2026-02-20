// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Server._Gabystation.CompactPod.Components;

[RegisterComponent]
public sealed partial class CompactPodPassengerComponent : Component
{
    [ViewVariables]
    public EntityUid Pod = EntityUid.Invalid;
}
