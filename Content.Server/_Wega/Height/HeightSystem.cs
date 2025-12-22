using System.Linq;
using System.Numerics;
using Content.Shared.Height;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared._EinsteinEngines.HeightAdjust;
using Robust.Shared.Prototypes;

namespace Content.Server.Height;

public sealed class HeightSystem : EntitySystem
{
    [Dependency] private readonly HeightAdjustSystem _heightAdjust = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmallHeightComponent, ComponentInit>(OnSmallInit);
        SubscribeLocalEvent<BigHeightComponent, ComponentInit>(OnBigInit);
        SubscribeLocalEvent<SmallHeightComponent, ComponentShutdown>(OnSmallShutdown);
        SubscribeLocalEvent<BigHeightComponent, ComponentShutdown>(OnBigShutdown);
    }

    private void OnSmallInit(Entity<SmallHeightComponent> ent, ref ComponentInit args)
    {
        if (ShouldIgnore(ent))
            return;

        ent.Comp.OriginalScale ??= Vector2.One;
        _heightAdjust.SetScale(ent.Owner, new Vector2(0.75f, 0.75f));
    }

    private void OnBigInit(Entity<BigHeightComponent> ent, ref ComponentInit args)
    {
        if (ShouldIgnore(ent))
            return;

        ent.Comp.OriginalScale ??= Vector2.One;
        _heightAdjust.SetScale(ent.Owner, new Vector2(1.25f, 1.25f));
    }

    private void OnSmallShutdown(Entity<SmallHeightComponent> ent, ref ComponentShutdown args)
    {
        var scale = ent.Comp.OriginalScale ?? Vector2.One;
        _heightAdjust.SetScale(ent.Owner, scale);
        ent.Comp.OriginalScale = null;
    }

    private void OnBigShutdown(Entity<BigHeightComponent> ent, ref ComponentShutdown args)
    {
        var scale = ent.Comp.OriginalScale ?? Vector2.One;
        _heightAdjust.SetScale(ent.Owner, scale);
        ent.Comp.OriginalScale = null;
    }

    private bool ShouldIgnore(EntityUid uid)
    {
        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            return false;

        var ignoredSpecies = new[]
        {
            "Dwarf",
        };

        return ignoredSpecies.Contains(humanoid.Species.ToString());
    }
}
