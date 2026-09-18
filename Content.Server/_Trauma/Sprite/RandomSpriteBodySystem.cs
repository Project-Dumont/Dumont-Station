// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Body.Part;
using Content.Shared._Trauma.Sprite;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Sprite;

namespace Content.Server._Trauma.Sprite;

/// <summary>
/// Applies random sprite colour to a mob's limbs.
/// </summary>
public sealed partial class RandomSpriteBodySystem : EntitySystem
{
    [Dependency] private SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomSpriteComponent, RandomSpriteChangedEvent>(OnSpriteChanged);
        SubscribeLocalEvent<RandomSpriteComponent, BodyPartAddedEvent>(OnPartAdded);
    }

    private void OnSpriteChanged(Entity<RandomSpriteComponent> ent, ref RandomSpriteChangedEvent args)
    {
        if (GetAnyColor(ent.Comp) is not {} color || !TryComp<BodyComponent>(ent, out var body))
            return;

        foreach (var part in _body.GetBodyChildren(ent, body))
        {
            SetColor(part.Id, color);
        }
    }

    private void OnPartAdded(Entity<RandomSpriteComponent> ent, ref BodyPartAddedEvent args)
    {
        if (GetAnyColor(ent.Comp) is {} color)
            SetColor(args.Part, color);
    }

    private void SetColor(EntityUid part, Color color)
    {
        var appearance = EnsureComp<BodyPartAppearanceComponent>(part);
        appearance.Color = color;
        Dirty(part, appearance);
    }

    // jesus christ the carp random color system is overengineered
    private Color? GetAnyColor(RandomSpriteComponent comp)
    {
        foreach (var pair in comp.Selected.Values)
        {
            if (pair.Color is {} color)
                return color;
        }

        return null;
    }
}
