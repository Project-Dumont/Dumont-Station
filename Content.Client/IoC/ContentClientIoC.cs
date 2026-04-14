// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Humanoid;

namespace Content.Client.IoC;

internal static class ContentClientIoC
{
    internal static void Register(IDependencyCollection collection)
    {
        collection.Register<ShaderMarkingManager>();
    }
}
