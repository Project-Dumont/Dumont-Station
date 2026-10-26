using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using System.Numerics;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphHudOverlay : Overlay
{
    [Dependency] private IEntityManager _entities = default!;
    [Dependency] private IPlayerManager _players = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IResourceCache _resources = default!;
    private static readonly ResPath Rsi = new("/Textures/_RMC14/Interface/xeno_hud.rsi");
    private static readonly ProtoId<ShaderPrototype> Shader = "unshaded";
    private readonly XenomorphHudSystem _hud;
    private readonly SpriteSystem _sprites;
    private readonly SharedTransformSystem _transform;
    private readonly ContainerSystem _containers;
    private readonly ShaderInstance _shader;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public XenomorphHudOverlay()
    {
        IoCManager.InjectDependencies(this);
        _hud = _entities.System<XenomorphHudSystem>();
        _sprites = _entities.System<SpriteSystem>();
        _transform = _entities.System<SharedTransformSystem>();
        _containers = _entities.System<ContainerSystem>();
        _shader = _prototypes.Index(Shader).Instance();
        ZIndex = 1;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_entities.HasComponent<XenomorphComponent>(_players.LocalEntity)) return;
        var handle = args.WorldHandle;
        var rotation = Matrix3Helpers.CreateRotation(-(args.Viewport.Eye?.Rotation ?? Angle.Zero));
        handle.UseShader(_shader);
        var query = _entities.AllEntityQueryEnumerator<XenomorphComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var sprite, out var xform))
        {
            if (xform.MapID != args.MapId || !sprite.Visible || !_hud.CanSeeBars(_players.LocalEntity, uid)
                || _containers.IsEntityOrParentInContainer(uid, xform: xform)) continue;
            var bounds = _sprites.GetLocalBounds((uid, sprite));
            var world = _transform.GetWorldPosition(xform);
            if (!bounds.Translated(world).Intersects(args.WorldAABB)) continue;
            handle.SetTransform(rotation * Matrix3x2.CreateTranslation(world));
            var position = new Vector2(-bounds.Width / 2 + sprite.Offset.X / 2 - 0.125f, -bounds.Height / 2 + sprite.Offset.Y / 2);
            DrawBar(handle, _hud.GetHealthState(uid), position);
            DrawBar(handle, _hud.GetShieldState(uid), position);
            DrawBar(handle, _hud.GetResourceState(uid), position);
            DrawBar(handle, _hud.GetHealingState(uid), position);
            DrawPheromones(uid, handle, bounds, sprite);
        }
        var stacks = _entities.AllEntityQueryEnumerator<XenomorphAcidStacksComponent, SpriteComponent, TransformComponent>();
        while (stacks.MoveNext(out var uid, out var acid, out var sprite, out var xform))
        {
            if (xform.MapID != args.MapId || !sprite.Visible || _containers.IsEntityOrParentInContainer(uid, xform: xform)) continue;
            var bounds = _sprites.GetLocalBounds((uid, sprite));
            var world = _transform.GetWorldPosition(xform);
            if (!bounds.Translated(world).Intersects(args.WorldAABB)) continue;
            handle.SetTransform(rotation * Matrix3x2.CreateTranslation(world));
            DrawBar(handle, "acid_stacks" + Math.Clamp(acid.Current, 1, 4), new Vector2(-bounds.Width / 2, bounds.Height / 2));
        }
        var intoxicated = _entities.AllEntityQueryEnumerator<XenomorphIntoxicatedComponent, SpriteComponent, TransformComponent>();
        while (intoxicated.MoveNext(out var uid, out var toxin, out var sprite, out var xform))
        {
            if (xform.MapID != args.MapId || !sprite.Visible || _containers.IsEntityOrParentInContainer(uid, xform: xform)) continue;
            var bounds = _sprites.GetLocalBounds((uid, sprite));
            var world = _transform.GetWorldPosition(xform);
            if (!bounds.Translated(world).Intersects(args.WorldAABB) || toxin.Stacks <= 0) continue;
            handle.SetTransform(rotation * Matrix3x2.CreateTranslation(world));
            var path = new ResPath("/Textures/_RMC14/Interface/Alerts/xeno_toxins.rsi");
            var position = new Vector2(-bounds.Width / 2, bounds.Height / 2);
            handle.DrawTexture(_sprites.GetFrame(new SpriteSpecifier.Rsi(path, "intoxicated_amount" + Math.Clamp(toxin.Stacks, 0, 30)), _timing.CurTime), position);
            handle.DrawTexture(_sprites.GetFrame(new SpriteSpecifier.Rsi(path, toxin.Stacks >= 20 ? "intoxicated_high" : "intoxicated"), _timing.CurTime), position);
            if (toxin.Stacks >= 20) handle.DrawTexture(_sprites.GetFrame(new SpriteSpecifier.Rsi(new ResPath("/Textures/_RMC14/Effects/xeno_stomp.rsi"), "stomp"), _timing.CurTime), position);
        }
        var marks = _entities.AllEntityQueryEnumerator<XenomorphDancerMarkComponent, SpriteComponent, TransformComponent>();
        while (marks.MoveNext(out var uid, out var mark, out var sprite, out var xform))
        {
            if (xform.MapID != args.MapId || !sprite.Visible || _containers.IsEntityOrParentInContainer(uid, xform: xform)) continue;
            var bounds = _sprites.GetLocalBounds((uid, sprite));
            var world = _transform.GetWorldPosition(xform);
            if (!bounds.Translated(world).Intersects(args.WorldAABB)) continue;
            handle.SetTransform(rotation * Matrix3x2.CreateTranslation(world));
            var path = new ResPath("/Textures/_CMU14/Interface/Hud/dancer_marks.rsi");
            var position = new Vector2(-bounds.Width / 2, bounds.Height / 2);
            if (mark.RedUntil > _timing.CurTime)
                handle.DrawTexture(_sprites.GetFrame(new SpriteSpecifier.Rsi(path, "prae_tag"), _timing.CurTime), position);
            if (mark.YellowUntil > _timing.CurTime)
                handle.DrawTexture(_sprites.GetFrame(new SpriteSpecifier.Rsi(path, "prae_tag_yellow"), _timing.CurTime), position);
        }
        handle.UseShader(null);
        handle.SetTransform(Matrix3x2.Identity);
    }

    private void DrawPheromones(EntityUid uid, DrawingHandleWorld handle, Box2 bounds, SpriteComponent sprite)
    {
        if (!_entities.TryGetComponent<XenomorphPheromoneEffectComponent>(uid, out var effect))
            return;

        var path = new ResPath("/Textures/_RMC14/Interface/xeno_pheromones_hud.rsi");
        var position = new Vector2((bounds.Width + sprite.Offset.X) / 2 - 1.25f,
            (bounds.Height + sprite.Offset.Y) / 2 - bounds.Height);
        if (effect.Recovery > 0)
            handle.DrawTexture(_sprites.GetFrame(new SpriteSpecifier.Rsi(path, "recovery"), _timing.CurTime), position);
        if (effect.Warding > 0)
            handle.DrawTexture(_sprites.GetFrame(new SpriteSpecifier.Rsi(path, "warding"), _timing.CurTime), position);
        if (effect.Frenzy > 0)
            handle.DrawTexture(_sprites.GetFrame(new SpriteSpecifier.Rsi(path, "frenzy"), _timing.CurTime), position);
        if (effect.Source is "recovery" or "warding" or "frenzy")
            handle.DrawTexture(_sprites.GetFrame(new SpriteSpecifier.Rsi(path, "aura_" + effect.Source), _timing.CurTime), position);
    }

    private void DrawBar(DrawingHandleWorld handle, string? state, Vector2 position)
    {
        if (state == null || !_resources.GetResource<RSIResource>(Rsi).RSI.TryGetState(state, out _)) return;
        var texture = _sprites.GetFrame(new SpriteSpecifier.Rsi(Rsi, state), _timing.CurTime);
        handle.DrawTexture(texture, position);
    }
}
