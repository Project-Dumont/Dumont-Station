// SPDX-FileCopyrightText: 2026 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Audio;
using Content.Shared._ES.CCVar;
using Content.Shared._ES.DeathCutscene;
using Robust.Client.Audio;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client._ES.DeathCutscene;

public sealed partial class DeathCutsceneSystem : EntitySystem
{
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private ContentAudioSystem _contentAudio = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IOverlayManager _overlay = default!;

    private DeathCutsceneOverlay? _current;
    private EntityUid? _sound;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<PlayDeathCutsceneEvent>(OnPlayDeathCutscene);
        SubscribeNetworkEvent<StopDeathCutsceneEvent>(OnStopDeathCutscene);
    }

    public override void FrameUpdate(float frameTime)
    {
        if (_current is { Finished: true })
            RemoveOverlay();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        RemoveOverlay();
    }

    private void OnPlayDeathCutscene(PlayDeathCutsceneEvent msg)
    {
        if (_current != null || !_cfg.GetCVar(ESCCVars.DeathCutscene))
            return;

        _current = new DeathCutsceneOverlay(msg.Timings, _timing.RealTime);
        _overlay.AddOverlay(_current);

        if (msg.SuppressAmbientMusic)
            _contentAudio.SetAmbientMusicSuppressed(true);

        _sound = _audio.PlayGlobal(msg.Sound, Filter.Local(), false)?.Entity;
    }

    private void OnStopDeathCutscene(StopDeathCutsceneEvent msg)
    {
        if (msg.StopSound)
            _sound = _audio.Stop(_sound);

        RemoveOverlay();
    }

    private void RemoveOverlay()
    {
        if (_current == null)
            return;

        _overlay.RemoveOverlay(_current);
        _current = null;

        _contentAudio.SetAmbientMusicSuppressed(false);
    }
}
