using Content.Client.Parallax;
using Content.Client.Parallax.Managers;
using Content.Shared._Dumont.Parallax;

namespace Content.Client._Dumont.Parallax;

public sealed partial class ExtraParallaxSystem : EntitySystem
{
    [Dependency] private IParallaxManager _parallax = default!;

    private readonly Dictionary<EntityUid, (ParallaxLayerPrepared[] Base, ParallaxLayerPrepared[] Combined)> _cache = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExtraParallaxComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<ExtraParallaxComponent, ComponentRemove>(OnRemove);
    }

    private void OnState(Entity<ExtraParallaxComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _cache.Remove(ent);
        foreach (var id in ent.Comp.Parallaxes)
        {
            if (!_parallax.IsLoaded(id))
                _parallax.LoadParallaxByName(id);
        }
    }

    private void OnRemove(Entity<ExtraParallaxComponent> ent, ref ComponentRemove args)
    {
        _cache.Remove(ent);
    }

    public ParallaxLayerPrepared[] AddLayers(EntityUid map, ParallaxLayerPrepared[] layers)
    {
        if (!TryComp<ExtraParallaxComponent>(map, out var comp) || comp.Parallaxes.Count == 0)
            return layers;

        if (_cache.TryGetValue(map, out var cached) && cached.Base == layers)
            return cached.Combined;

        var combined = new List<ParallaxLayerPrepared>(layers);
        var loaded = true;
        foreach (var id in comp.Parallaxes)
        {
            if (!_parallax.IsLoaded(id))
            {
                loaded = false;
                continue;
            }

            combined.AddRange(_parallax.GetParallaxLayers(id));
        }

        var result = combined.ToArray();
        if (loaded)
            _cache[map] = (layers, result);

        return result;
    }
}
