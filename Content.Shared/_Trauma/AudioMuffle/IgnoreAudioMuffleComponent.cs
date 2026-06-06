// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.AudioMuffle;

/// <summary>
/// Ignore audio muffle.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IgnoreAudioMuffleComponent : Component;
