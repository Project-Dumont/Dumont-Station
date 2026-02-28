// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Containers;

namespace Content.Server._Gabystation.CompactPod.Components;

[RegisterComponent]
public sealed partial class CompactPodComponent : Component
{
    [DataField]
    public int MaxPassengers = 1;

    public Container PassengerContainer = default!;

    [ViewVariables]
    public readonly string PassengerContainerId = "pod-passenger-container";
}
