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

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Rust;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class EntropicPlumeAffectedComponent : BaseSpriteOverlayComponent
{
    [DataField, AutoNetworkedField]
    public EntityUid ExcludedEntity;

    // Null for infinite
    [DataField]
    public float? Duration = 10f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextAttack = TimeSpan.Zero;

    public override Enum Key { get; set; } = EntropicPlumeKey.Key;

    [DataField, AutoNetworkedField]
    public override SpriteSpecifier? Sprite { get; set; } =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/Effects/effects.rsi"), "cloud_swirl");
}

public enum EntropicPlumeKey : byte
{
    Key,
}
