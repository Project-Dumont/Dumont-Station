// Dumont start
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Analyzers;
using Robust.Shared.Log;
using Robust.Shared.Localization;
using Robust.Shared.GameStates;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
// Dumont end

namespace Content.Client.Graphics;

public static class ContentPostShaderIds
{
    public const string Stealth = "stealth";
    public const string FloorOcclusion = "floor-occlusion";
    public const string Holopad = "holopad";
    public const string InteractionOutline = "interaction-outline";
    public const string TargetOutline = "target-outline";
    public const string DragDropOutline = "drag-drop-outline";

    public static readonly string[] BeforeOutlines =
    {
        InteractionOutline,
        TargetOutline,
        DragDropOutline,
    };

    public static readonly string[] AfterBaseEffects =
    {
        Stealth,
        FloorOcclusion,
        Holopad,
    };
}
