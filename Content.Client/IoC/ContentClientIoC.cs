// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Humanoid;
using Robust.Shared.IoC;

namespace Content.Client.IoC;

internal static class ContentClientIoC
{
    internal static void Register()
    {
        IoCManager.Register<ShaderMarkingManager>();
    }
}
