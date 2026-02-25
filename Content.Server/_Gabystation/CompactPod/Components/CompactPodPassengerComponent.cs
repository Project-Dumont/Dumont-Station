// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Gabystation.CompactPod.Components;

[RegisterComponent]
public sealed partial class CompactPodPassengerComponent : Component
{
    [ViewVariables]
    public EntityUid Pod;
}
