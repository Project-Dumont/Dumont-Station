// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Geras;

/// <summary>
/// Geras is the god of old age, and A geras is the small morph of a slime. This system allows the slimes to have the morphing action.
/// </summary>
public abstract class SharedGerasSystem : EntitySystem;

public sealed partial class MorphIntoGeras : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class MorphIntoGerasDoAfterEvent : SimpleDoAfterEvent;
