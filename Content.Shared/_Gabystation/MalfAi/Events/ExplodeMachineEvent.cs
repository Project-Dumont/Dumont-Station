using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._Gabystation.MalfAi;

public sealed partial class ExplodeMachineEvent : EntityTargetActionEvent
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
