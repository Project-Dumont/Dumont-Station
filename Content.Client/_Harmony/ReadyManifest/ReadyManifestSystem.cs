// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Harmony.ReadyManifest;

namespace Content.Client._Harmony.ReadyManifest;

public sealed class ReadyManifestSystem : SharedReadyManifestSystem
{
    public void RequestReadyManifest()
    {
        RaiseNetworkEvent(new RequestReadyManifestMessage());
    }
}
