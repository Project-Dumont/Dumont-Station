// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Void;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class VoidCurseComponent : BaseSpriteOverlayComponent
{
    [DataField]
    public float Lifetime = 5f; // 6s on 1 stack, 12s on max stack

    [DataField]
    public float MaxLifetime = 5f;

    [DataField]
    public float LifetimeIncreasePerLevel = 1f;

    [DataField, AutoNetworkedField]
    public float Stacks;

    [DataField]
    public float MinStacksToMute = 4f;

    [DataField]
    public float MaxStacks = 7f;

    [DataField]
    public TimeSpan Timer = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public string OverlayStateNormal = "void_chill_partial";

    [DataField]
    public string OverlayStateMax = "void_chill_oh_fuck";

    public override Enum Key { get; set; } = VoidCurseKey.Key;

    [DataField]
    public override SpriteSpecifier? Sprite { get; set; } =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/void_overlay.rsi"), "void_chill_partial");
}

public enum VoidCurseKey : byte
{
    Key,
}
