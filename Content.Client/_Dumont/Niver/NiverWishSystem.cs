// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Dumont.Niver;

namespace Content.Client._Dumont.Niver;

public sealed partial class NiverWishSystem : EntitySystem
{
    private NiverWishWindow? _window;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<NiverWishRequestEvent>(OnRequest);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _window?.Close();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_window is { TimedOut: true })
            _window.Close();
    }

    private void OnRequest(NiverWishRequestEvent ev)
    {
        _window?.Close();

        _window = new NiverWishWindow(ev.Duration);
        _window.OnAnswered += wished => RaiseNetworkEvent(new NiverWishResponseEvent(wished));
        _window.OnClose += () => _window = null;
        _window.OpenCentered();
    }
}
