// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Harmony.Objectives.Systems;

namespace Content.Server._Harmony.Objectives.Components;

/// <summary>
/// This objective will always show as 0% complete as it is not intended to be tracked. Used for Conspirator objectives.
/// </summary>
[RegisterComponent, Access(typeof(NoCompletionObjectiveSystem))]
public sealed partial class NoCompletionObjectiveComponent : Component;
