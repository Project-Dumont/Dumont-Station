// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// This component, analogous to <see cref="BatterySelfRechargerComponent"/>, will refill its owner's
/// <see cref="BallisticAmmoProviderComponent"/> over time.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class BallisticAmmoSelfRefillerComponent : Component
{
    /// <summary>
    /// True if the refilling behavior is active, false otherwise.
    /// </summary>
    [DataField]
    public bool AutoRefill = true;

    /// <summary>
    /// How often a new piece of ammunition is inserted into the owner's <see cref="BallisticAmmoProviderComponent"/>.
    /// </summary>
    [DataField]
    public TimeSpan AutoRefillRate = TimeSpan.FromSeconds(1);

    /// <summary>
    /// What entity to spawn and attempt to insert into the owner. If null, uses
    /// <see cref="BallisticAmmoProviderComponent.Proto"/>.
    /// </summary>
    [DataField]
    public EntProtoId? AmmoProto;

    /// <summary>
    /// If true, EMPs will pause this component's behavior.
    /// </summary>
    [DataField]
    public bool AffectedByEmp;

    /// <summary>
    /// When the next auto refill should occur.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextAutoRefill = TimeSpan.Zero;

    /// <summary>
    /// Optional sound that plays when this item successfully refills a piece of ammo.
    /// </summary>
    [DataField]
    public SoundSpecifier? RechargeSound;
}
