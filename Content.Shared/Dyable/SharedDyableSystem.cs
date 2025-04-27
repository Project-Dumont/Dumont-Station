using System.Linq;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Item;
using Content.Shared.WashingMachine.Events;
using JetBrains.Annotations;
using Robust.Shared.Reflection;
using Vector4 = System.Numerics.Vector4;

namespace Content.Shared.Dyable;

public abstract partial class SharedDyableSystem : EntitySystem
{
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly IReflectionManager _reflection = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DyableComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<DyableComponent, WashingMachineIsBeingWashed>(OnWashed);
    }

    private void OnInit(Entity<DyableComponent> ent, ref ComponentInit args)
    {
        Appearance.SetData(ent.Owner, DyableVisuals.Color, ent.Comp.Color);
        UpdateClothing(ent);
        UpdateItem(ent);
        UpdateForensics(ent);
    }

    private void OnWashed(Entity<DyableComponent> ent, ref WashingMachineIsBeingWashed args)
    {
        HashSet<Color> colors = new();

        foreach (var item in args.Items)
        {
            var ev = new GetDyableColorsEvent();
            RaiseLocalEvent(item, ev);

            if (!ev.Handled)
                continue;

            colors.Add(ev.Color);
        }

        if (!colors.Any())
            return;

        SetColor((ent.Owner, ent.Comp), MixColors(colors));
    }

    private static Color MixColors(HashSet<Color> colors)
    {
        if (colors.Count == 1)
            return colors.First();

        float r = 1f, g = 1f, b = 1f;

        foreach (var color in colors)
        {
            r *= 1f - color.R;
            g *= 1f - color.G;
            b *= 1f - color.B;
        }

        return new Color(1f - r, 1f - g, 1f - b);
    }

    private void UpdateClothing(Entity<DyableComponent> ent)
    {
        if (!TryComp<ClothingComponent>(ent.Owner, out var clothing))
            return;

        var key = _reflection.GetEnumReference(DyableVisualsLayers.Layer);

        foreach (var slot in clothing.ClothingVisuals.Keys)
            _clothing.SetLayerColor(clothing, slot, key, ent.Comp.Color);
    }

    private void UpdateItem(Entity<DyableComponent> ent)
    {
        if (!TryComp<ItemComponent>(ent.Owner, out var item))
            return;

        var key = _reflection.GetEnumReference(DyableVisualsLayers.Layer);

        foreach (var hand in item.InhandVisuals.Keys)
            _item.SetLayerColor(ent.Owner, hand, key, ent.Comp.Color, item);
    }

    protected virtual void UpdateForensics(Entity<DyableComponent> ent)
    {
    }

    [PublicAPI]
    public void SetColor(Entity<DyableComponent?> ent, Color color)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        ent.Comp.Color = color;
        DirtyField(ent.Owner, ent.Comp, nameof(DyableComponent.Color));
        Appearance.SetData(ent.Owner, DyableVisuals.Color, ent.Comp.Color);
        UpdateClothing((ent.Owner, ent.Comp));
        UpdateItem((ent.Owner, ent.Comp));
        UpdateForensics((ent.Owner, ent.Comp));
    }
}
