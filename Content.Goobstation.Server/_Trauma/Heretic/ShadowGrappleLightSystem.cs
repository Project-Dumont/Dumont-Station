using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Trauma.Shared.ShadowDemon;
using Robust.Shared.Containers;

namespace Content.Trauma.Server.Heretic;

public sealed class ShadowGrappleLightSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private PoweredLightSystem _lights = default!;
    [Dependency] private SharedContainerSystem _containers = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowGrappleBreakLightsEvent>(OnBreakLights);
    }

    private void OnBreakLights(ref ShadowGrappleBreakLightsEvent args)
    {
        var lights = new HashSet<Entity<PoweredLightComponent>>();
        _lookup.GetEntitiesInRange(Transform(args.Target).Coordinates, args.Range, lights);
        foreach (var light in lights)
        {
            if (_containers.TryGetContainingContainer(light.Owner, out var container)
                && !_containers.CanRemove(light.Owner, container))
                continue;

            _lights.TryDestroyBulb(light.Owner, light.Comp);
        }
    }
}
