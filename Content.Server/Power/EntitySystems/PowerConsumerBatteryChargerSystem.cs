// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.Components;

namespace Content.Server.Power.EntitySystems;

public sealed class PowerConsumerBatteryChargerSystem : EntitySystem
{
    [Dependency] private readonly BatterySystem _battery = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PowerConsumerBatteryChargerComponent, PowerConsumerComponent, BatteryComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var charger, out var consumer, out _, out var xform))
        {
            if (!xform.Anchored)
                continue;

            var powerConsumed = consumer.ReceivedPower * frameTime;
            _battery.ChangeCharge(uid, powerConsumed * charger.Efficiency);
        }
    }
}
