// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Light.EntitySystems;
using Content.Shared.EntityEffects;
using Content.Server.Light.Components;
using Content.Trauma.Shared.EntityEffects;

namespace Content.Trauma.Server.EntityEffects;

public sealed partial class DestroyLightBulbEffectSystem : EntitySystem
{
    [Dependency] private PoweredLightSystem _light = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<DestroyLightBulb>>(OnEffect);
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<DestroyLightBulb> args)
    {
        var uid = args.Args.TargetEntity;
        if (TryComp<PoweredLightComponent>(uid, out var light))
            _light.TryDestroyBulb(uid, light);
    }
}
