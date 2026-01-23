// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server._Gabystation;

// A existência disso é terrível...
public static class GabyConstants
{
    // Não sei onde colocar isso. Os GameRuleSystem<T> são genéricos, não da pra por neles.
    public static readonly ProtoId<TagPrototype> GameDirectorRuleTag = "GameDirectorRule";
}
