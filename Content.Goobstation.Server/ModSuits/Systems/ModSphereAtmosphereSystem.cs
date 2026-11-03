using Content.Goobstation.Shared.ModSuits.Modules;
using Content.Server.Atmos.EntitySystems;

namespace Content.Goobstation.Server.ModSuits.Systems;

public sealed class ModSphereAtmosphereSystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmosphere = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ModSpherePressureCheckEvent>(OnPressureCheck);
    }

    private void OnPressureCheck(ref ModSpherePressureCheckEvent args)
    {
        if (_atmosphere.GetTileMixture(args.User) is { } mixture)
            args.ExceedsLimit = mixture.Pressure > args.MaxPressure;
    }
}
