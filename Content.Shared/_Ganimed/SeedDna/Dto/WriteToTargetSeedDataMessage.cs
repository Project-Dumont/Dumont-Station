// SPDX-FileCopyrightText: 2025 CrimeMoot <wakeafa@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

// ReSharper disable once CheckNamespace
namespace Content.Shared._Ganimed.SeedDna;

[Serializable, NetSerializable]
public sealed class WriteToTargetSeedDataMessage(
    TargetSeedData target,
    SeedDataDto seedDataDto
) : BoundUserInterfaceMessage
{
    public readonly TargetSeedData Target = target;
    public readonly SeedDataDto SeedDataDto = seedDataDto;
}

[Serializable, NetSerializable]
public enum TargetSeedData : byte
{
    Seed,
    DnaDisk,
}
