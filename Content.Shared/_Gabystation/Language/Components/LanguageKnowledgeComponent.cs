// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared._Gabystation.Language.Components;

/// <summary>
///     Stores data about entities' intrinsic language knowledge.
/// </summary>
[RegisterComponent]
public sealed partial class LanguageKnowledgeComponent : Component
{
    /// <summary>
    ///     List of languages this entity can speak without any external tools.
    /// </summary>
    [DataField("speaks", customTypeSerializer: typeof(PrototypeIdListSerializer<LanguagePrototype>), required: true)]
    public List<string> SpokenLanguages = new();

    /// <summary>
    ///     List of languages this entity can understand without any external tools.
    /// </summary>
    [DataField("understands", customTypeSerializer: typeof(PrototypeIdListSerializer<LanguagePrototype>), required: true)]
    public List<string> UnderstoodLanguages = new();
}
