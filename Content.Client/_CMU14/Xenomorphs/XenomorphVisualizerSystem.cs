using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Client.DamageState;
using Robust.Shared.Utility;
using Robust.Client.Graphics;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Throwing;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.GameObjects;
using Robust.Shared.Physics.Components;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphVisualizerSystem : VisualizerSystem<XenomorphRestComponent>
{
    [Dependency] private Robust.Client.Player.IPlayerManager _player = default!;
    [Dependency] private MobStateSystem _mobs = default!;
    [Dependency] private Robust.Shared.Timing.IGameTiming _timing = default!;

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        var query = EntityQueryEnumerator<XenomorphRestComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var rest, out var sprite))
        {
            var appearance = new AppearanceChangeEvent { Sprite = sprite, Component = Comp<AppearanceComponent>(uid) };
            OnAppearanceChange(uid, rest, ref appearance);
            if (HasComp<XenomorphDrainSurgeComponent>(uid)) SpriteSystem.SetColor((uid, sprite), Color.Chartreuse);
            if (TryComp<XenomorphHealerComponent>(uid, out var healer)
                && SpriteSystem.LayerMapTryGet((uid, sprite), "healer-salve", out var salveLayer, false))
            {
                var visible = healer.SalvingUntil > _timing.CurTime && !_mobs.IsDead(uid);
                SpriteSystem.LayerSetRsiState((uid, sprite), salveLayer, rest.Resting ? "salved_rest" : !_mobs.IsAlive(uid) ? "salved_downed" : "salved");
                SpriteSystem.LayerSetVisible((uid, sprite), salveLayer, visible);
            }
            if (TryComp<XenomorphAciderComponent>(uid, out var acider))
            {
                UpdateAcider(uid, sprite, rest, acider);
                continue;
            }
            if (HasComp<XenomorphHedgehogComponent>(uid)) continue;
            if (!_mobs.IsAlive(uid) || rest.Resting || sprite.BaseRSI is not { } rsi
                || !SpriteSystem.LayerMapTryGet((uid, sprite), DamageStateVisualLayers.Base, out var layer, false))
                continue;
            var state = "alive";
            if (TryComp<XenomorphBulwarkComponent>(uid, out var bulwark) && bulwark.Encased && rsi.TryGetState("shield", out _))
                state = "shield";
            else if (HasComp<ThrownItemComponent>(uid) && rsi.TryGetState("thrown", out _))
                state = "thrown";
            else if (((TryComp<XenomorphChargeComponent>(uid, out var charge) && charge.Active)
                || (TryComp<PhysicsComponent>(uid, out var physics) && physics.LinearVelocity.LengthSquared() > 0.01f)
                || (TryComp<InputMoverComponent>(uid, out var input) && (input.HeldMoveButtons & MoveButtons.AnyDirection) != MoveButtons.None))
                && rsi.TryGetState("running", out _))
                state = "running";
            SpriteSystem.LayerSetRsiState((uid, sprite), layer, state);
        }
    }

    private void UpdateAcider(EntityUid uid, SpriteComponent sprite, XenomorphRestComponent rest, XenomorphAciderComponent acider)
    {
        var path = new ResPath(acider.Primed
            ? "/Textures/_CMU14/Mobs/Xenomorphs/acider_runner_primed.rsi"
            : "/Textures/_CMU14/Mobs/Xenomorphs/acider_runner.rsi");
        if (SpriteSystem.LayerMapTryGet((uid, sprite), DamageStateVisualLayers.Base, out var baseLayer, false))
        {
            SpriteSystem.LayerSetRsi((uid, sprite), baseLayer, path);
            SpriteSystem.LayerSetRsiState((uid, sprite), baseLayer, _mobs.IsDead(uid) ? "dead" : !_mobs.IsAlive(uid) ? "crit" : "alive");
            if (acider.Primed) SpriteSystem.LayerSetAnimationTime((uid, sprite), baseLayer,
                1.6f * Math.Clamp((float)(acider.DetonateAt - _timing.CurTime).TotalSeconds / 20f, 0, 1));
        }
        if (SpriteSystem.LayerMapTryGet((uid, sprite), "rest", out var restLayer, false))
            SpriteSystem.LayerSetRsi((uid, sprite), restLayer, path);
        if (!SpriteSystem.LayerMapTryGet((uid, sprite), "acider-acid", out var acidLayer, false))
        {
            acidLayer = SpriteSystem.AddRsiLayer((uid, sprite), new RSI.StateId("acid"), path);
            SpriteSystem.LayerMapSet((uid, sprite), "acider-acid", acidLayer);
        }
        SpriteSystem.LayerSetRsi((uid, sprite), acidLayer, path);
        SpriteSystem.LayerSetRsiState((uid, sprite), acidLayer, rest.Resting ? "acid_rest" : !_mobs.IsAlive(uid) ? "acid_downed" : "acid");
        SpriteSystem.LayerSetVisible((uid, sprite), acidLayer, acider.GeneratingUntil > _timing.CurTime && !_mobs.IsDead(uid));

    }

    protected override void OnAppearanceChange(EntityUid uid, XenomorphRestComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        AppearanceSystem.TryGetData<bool>(uid, XenomorphVisuals.Invisible, out var invisible, args.Component);
        if (TryComp<XenomorphHideComponent>(uid, out var hide))
        {
            invisible = false;
        }
        var burrowed = TryComp<XenomorphBurrowComponent>(uid, out var burrow) && burrow.Active;
        SpriteSystem.SetVisible((uid, args.Sprite), !burrowed || _player.LocalEntity == uid);
        SpriteSystem.SetColor((uid, args.Sprite), args.Sprite.Color.WithAlpha(burrowed ? 0.4f : invisible ? 0.1f : 1f));
        AppearanceSystem.TryGetData<bool>(uid, XenomorphVisuals.Resting, out var resting, args.Component);
        AppearanceSystem.TryGetData<bool>(uid, XenomorphVisuals.Ovipositor, out var attached, args.Component);

        AppearanceSystem.TryGetData<bool>(uid, XenomorphVisuals.Crest, out var crested, args.Component);
        AppearanceSystem.TryGetData<bool>(uid, XenomorphVisuals.Fortified, out var fortified, args.Component);
        if (SpriteSystem.LayerMapTryGet((uid, args.Sprite), DamageStateVisualLayers.Base, out var baseLayer, false))
            SpriteSystem.LayerSetVisible((uid, args.Sprite), baseLayer, !resting && !attached && !fortified && !crested);
        if (SpriteSystem.LayerMapTryGet((uid, args.Sprite), "crest", out var crestLayer, false))
            SpriteSystem.LayerSetVisible((uid, args.Sprite), crestLayer, crested && !resting && !attached && !fortified);
        if (SpriteSystem.LayerMapTryGet((uid, args.Sprite), "fortify", out var fortifyLayer, false))
            SpriteSystem.LayerSetVisible((uid, args.Sprite), fortifyLayer, fortified);
        if (SpriteSystem.LayerMapTryGet((uid, args.Sprite), "rest", out var restLayer, false))
            SpriteSystem.LayerSetVisible((uid, args.Sprite), restLayer, resting && !attached);
        if (SpriteSystem.LayerMapTryGet((uid, args.Sprite), "ovipositor", out var eggLayer, false))
            SpriteSystem.LayerSetVisible((uid, args.Sprite), eggLayer, attached);
        if (TryComp<XenomorphHedgehogComponent>(uid, out var hedgehog)
            && SpriteSystem.LayerMapTryGet((uid, args.Sprite), DamageStateVisualLayers.Base, out var hedgehogLayer, false))
        {
            var level = hedgehog.Shards < 50 ? 1 : hedgehog.Shards < 150 ? 2 : hedgehog.Shards < 300 ? 3 : 4;
            var state = _mobs.IsDead(uid) ? "dead" : $"hedgehog_{level}" + (!_mobs.IsAlive(uid) ? "_crit" : resting ? "_resting" : "");
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), hedgehogLayer, state);
            SpriteSystem.LayerSetVisible((uid, args.Sprite), hedgehogLayer, true);
            if (SpriteSystem.LayerMapTryGet((uid, args.Sprite), "rest", out var sleepingLayer, false))
                SpriteSystem.LayerSetVisible((uid, args.Sprite), sleepingLayer, false);
        }
    }
}
