using Content.Shared.GameTicking;
using Robust.Shared.Map;

namespace Content.Server.Genetics.System;

public sealed partial class DnaModifierSystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;

    public EntityUid? PausedMap { get; private set; }

    private void InitializeMap()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent _)
    {
        if (PausedMap == null || !Exists(PausedMap))
            return;

        Del(PausedMap.Value);
    }

    private void EnsurePausedMap()
    {
        if (PausedMap != null && Exists(PausedMap))
            return;

        var newmap = _mapManager.CreateMap();
        _mapManager.SetMapPaused(newmap, true);
        PausedMap = _mapManager.GetMapEntityId(newmap);
    }
}
