// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
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