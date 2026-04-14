// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;

namespace Content.Shared.Humanoid.Markings;

public sealed partial class MarkingManager
{
    public Func<MarkingPrototype, int, MarkingSet, Dictionary<string, Vector3>?>? GetMarkingShaderParams;
}
