// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Trauma.Client.Humanoid;

namespace Content.Trauma.Client.IoC;

internal static class ContentTraumaClientIoC
{
    internal static void Register(IDependencyCollection collection)
    {
        collection.Register<ShaderMarkingManager>();
    }
}
