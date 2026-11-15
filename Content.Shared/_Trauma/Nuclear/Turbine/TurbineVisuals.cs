// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Trauma.Nuclear.Turbine;

/// <summary>
/// Appearance keys for the turbine.
/// </summary>
[Serializable, NetSerializable]
public enum TurbineVisuals : byte
{
    TurbineRuined,
    DamageSpark,
    DamageSmoke,
    TurbineSpeed,
}

/// <summary>
/// Visual sprite layers for the turbine.
/// </summary>
[Serializable, NetSerializable]
public enum TurbineVisualLayers : byte
{
    TurbineRuined,
    DamageSpark,
    DamageSmoke,
    TurbineSpeed,
}
