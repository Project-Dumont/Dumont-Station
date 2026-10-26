using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.Mobs.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphFruitVisualizerSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    private const string AuraId = "DumontXenomorphFruit";
    private static readonly ProtoId<ShaderPrototype> Aura = "DumontXenomorphAura";

    public static Color FruitColor(XenomorphFruitKind kind) => Color.FromHex(kind switch
    {
        XenomorphFruitKind.Greater => "#17991B",
        XenomorphFruitKind.Plasma => "#287A90",
        XenomorphFruitKind.Speed => "#5B248C",
        XenomorphFruitKind.Spore => "#994617",
        XenomorphFruitKind.Unstable => "#179973",
        _ => "#17991B"
    });

    public override void FrameUpdate(float frameTime)
    {
        var fruits = EntityQueryEnumerator<XenomorphFruitComponent, SpriteComponent>();
        while (fruits.MoveNext(out var uid, out var fruit, out var sprite))
        {
            if (fruit.State != XenomorphFruitState.Grown)
            {
                _sprite.RemovePostShader(sprite, AuraId);
                continue;
            }
            if (!_sprite.TryGetPostShader(sprite, AuraId, out var entry))
            {
                _sprite.SetPostShader(sprite, new SpriteComponent.PostShaderArgs(AuraId, _prototypes.Index(Aura).InstanceUnique()));
                if (!_sprite.TryGetPostShader(sprite, AuraId, out entry)) continue;
            }
            entry.Shader.SetParameter("outline_color", FruitColor(fruit.Kind).WithAlpha(0.5f));
            entry.Shader.SetParameter("outline_width", 1f);
        }
        var planters = EntityQueryEnumerator<XenomorphFruitPlanterComponent, SpriteComponent>();
        while (planters.MoveNext(out var uid, out var planter, out var sprite))
        {
            if (!_sprite.LayerMapTryGet((uid, sprite), "fruit-planter", out var layer, false)) continue;
            _sprite.LayerSetVisible((uid, sprite), layer, planter.Choice != null);
            if (planter.Choice is not { } choice) continue;
            var state = _mobs.IsDead(uid) ? "gardener_dead" : !_mobs.IsAlive(uid) ? "gardener_downed"
                : TryComp<XenomorphRestComponent>(uid, out var rest) && rest.Resting ? "gardener_rest" : "gardener_walk";
            _sprite.LayerSetRsiState((uid, sprite), layer, state);
            _sprite.LayerSetColor((uid, sprite), layer, FruitColor(choice));
        }
    }
}
