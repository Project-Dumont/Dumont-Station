// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.ADT.Geras;

[Access(typeof(SharedGerasSystem))]
public abstract partial class SharedGerasComponent : Component;

[Serializable, NetSerializable]
public enum GeraColor
{
    Color,
}
