// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Projectiles;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Components.PathSpecific.Lock;

/// <summary>
/// Added to doors trapped by lock path's serpentclave item
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class LockTrappedDoorComponent : Component
{
    [DataField]
    public EntProtoId<ProjectileComponent> ProjectileProto = "TentacleProjectile";

    [DataField]
    public SoundSpecifier DamageSound = new SoundCollectionSpecifier("ShadowDemonPunch");

    [DataField]
    public SoundSpecifier DeathSound = new SoundPathSpecifier("/Audio/_Trauma/Mobs/ShadowDemon/shadowdeath.ogg");

    [DataField]
    public SoundSpecifier AggroSound = new SoundCollectionSpecifier("ShadowDemonLaugh");

    [DataField]
    public TimeSpan JitterTime = TimeSpan.FromSeconds(3);

    [DataField]
    public FixedPoint2 DeathThreshold = 30;

    [DataField, AutoNetworkedField]
    public FixedPoint2 SustainedDamage;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan NextGrapple;

    [DataField]
    public TimeSpan GrappleDelay = TimeSpan.FromMilliseconds(300);

    [DataField, AutoNetworkedField]
    public EntityUid? GrappleTarget;

    [DataField]
    public SpriteSpecifier JointSprite =
        new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "tentacle");
}
