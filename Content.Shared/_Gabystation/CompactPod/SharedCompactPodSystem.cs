// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared._Gabystation.CompactPod;

public abstract partial class SharedCompactPodSystem : EntitySystem;

/// <summary>
/// Event raised when a person enters a pod, on both success and failure
/// </summary>
[Serializable, NetSerializable]
public sealed partial class PodPassengerEntryEvent : SimpleDoAfterEvent;
