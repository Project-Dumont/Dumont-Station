// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.MalfAi;

public sealed partial class ExplodeMachineEvent : WorldTargetActionEvent
{
    [DataField]
    public float Radius = 2;

    [DataField]
    public float Slope = 1;

    [DataField]
    public float MaxIntensity = 4;

    public ExplodeMachineEvent(float radius, float slope, float maxIntensity)
    {
        Radius = radius;
        Slope = slope;
        MaxIntensity = maxIntensity;
    }
}
