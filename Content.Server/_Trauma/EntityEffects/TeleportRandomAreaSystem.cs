// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.EntityEffects;
using Content.Trauma.Shared.Areas;
using Content.Trauma.Shared.EntityEffects;
using Robust.Shared.Random;

namespace Content.Trauma.Server.EntityEffects;

public sealed partial class TeleportRandomAreaSystem : EntitySystem
{
    [Dependency] private AreaSystem _area = default!;
    [Dependency] private AtmosphereSystem _atmos = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public const int Oxygen = (int) Gas.Oxygen;

    private List<Entity<TransformComponent>> _areas = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<TeleportRandomArea>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<TeleportRandomArea> args)
    {
        var uid = args.Args.TargetEntity;
        var map = Transform(uid).MapID;
        // TODO: add area teleport blacklist check if its needed for anything in the future
        // Dumont changes start
        Predicate<Entity<TransformComponent>> pred = args.Effect.Safe
            ? area => !IsTileUnsafe(area)
            : _ => true;
        // Dumont end
        _areas.Clear();
        _area.AddOpenAreas(map, _areas, args.Effect.CompName, pred);
        if (_areas.Count == 0)
            return;

        var area = _random.PickAndTake(_areas);
        _transform.SetCoordinates(uid, area.Comp.Coordinates); // Dumont
    }

    private bool IsTileUnsafe(Entity<TransformComponent> area)
        => IsMixtureUnsafe(_atmos.GetTileMixture(area.AsNullable())); // Dumont

    // Dumont changes start
    public static bool IsMixtureUnsafe(GasMixture? gas)
        => gas is not { } mixture || // space
            mixture.Temperature <= 270 || mixture.Temperature >= 360 || // bad temp
            mixture.Pressure <= 20 || mixture.Pressure >= 300 || // bad pressure
            mixture[Oxygen] < 16; // not enough oxygen
    // Dumont end
}
