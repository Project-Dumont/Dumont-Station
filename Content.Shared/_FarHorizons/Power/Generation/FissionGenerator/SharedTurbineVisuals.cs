// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later


using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

/// <summary>
/// Appearance keys for the turbine.
/// </summary>
[Serializable, NetSerializable]
public enum TurbineVisuals
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
public enum TurbineVisualLayers
{
    TurbineRuined,
    DamageSpark,
    DamageSmoke,
    TurbineSpeed,
}
