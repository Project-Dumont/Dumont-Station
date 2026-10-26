using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Robust.Client.GameObjects;

namespace Content.Client._CMU14.Xenomorphs;

public sealed class XenomorphCarrionShieldVisualizerSystem : VisualizerSystem<XenomorphCarrionShieldVisualComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, XenomorphCarrionShieldVisualComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null || !AppearanceSystem.TryGetData<float>(uid, XenomorphReaperVisuals.ShieldFraction, out var fraction, args.Component))
            return;
        var state = fraction > 0.5f ? "full" : fraction > 0.25f ? "half" : "quarter";
        SpriteSystem.LayerSetRsiState((uid, args.Sprite), 0, "king-shield-" + state);
    }
}
