// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Trauma.Shared.Areas;
using Robust.Client.GameObjects;
using Robust.Shared.Map;

namespace Content.Trauma.Client.Areas;

/// <summary>
/// Controls visibility of areas via the <c>showareas</c> and mapping commands.
/// </summary>
public sealed partial class AreaVisibilitySystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    private bool _visible;

    public bool Visible => _visible;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AreaComponent, ComponentInit>(OnInit);
    }

    public void SetVisible(bool visible)
    {
        if (_visible == visible)
            return;

        _visible = visible;
        UpdateAreas();
    }

    public void ToggleVisibility()
    {
        SetVisible(!_visible);
    }

    private void OnInit(Entity<AreaComponent> ent, ref ComponentInit args)
    {
        UpdateVisibility(ent);
    }

    private void UpdateVisibility(EntityUid uid)
    {
        // don't hide them in the spawnmenu
        if (Transform(uid).MapID == MapId.Nullspace && IsClientSide(uid))
            return;

        _sprite.SetVisible(uid, _visible);
    }

    private void UpdateAreas()
    {
        var query = AllEntityQuery<AreaComponent>(); // include paused for mapping
        while (query.MoveNext(out var uid, out _))
        {
            UpdateVisibility(uid);
        }
    }
}
