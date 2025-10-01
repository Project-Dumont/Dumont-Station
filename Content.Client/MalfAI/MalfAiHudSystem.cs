// SPDX-FileCopyrightText: 2025 Dreykor <Dreykor12@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 funkystationbot <funky@funkystation.org>
//
// SPDX-License-Identifier: MIT

using Content.Client.Alerts;
using Content.Shared.MalfAI;
using Content.Shared.Store.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Content.Shared.Silicons.StationAi;
using Robust.Client.GameObjects;

namespace Content.Client.MalfAI;

public sealed class MalfAiHudSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Subscribe on StationAiHeld so this runs for the local AI eye entity.
        SubscribeLocalEvent<StationAiHeldComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    private EntityUid? ResolveMalfAiEntity(EntityUid local)
    {
        // Prefer local if it already has a store (covers some setups)
        if (TryComp<StoreComponent>(local, out _))
            return local;

        // Find any entity flagged as Malf AI that also has a store.
        var query = AllEntityQuery<MalfAiMarkerComponent, StoreComponent>();
        while (query.MoveNext(out var uid, out _, out _))
            return uid;

        return null;
    }

    private void OnUpdateAlert(Entity<StationAiHeldComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != "MalfCpu")
            return;

        // Find which entity holds the CPU store.
        var source = ResolveMalfAiEntity(ent.Owner);
        if (source == null || !TryComp<StoreComponent>(source.Value, out var store))
            return;

        // Read CPU amount and clamp to 0..999
        ProtoId<CurrencyPrototype> cpu = "CPU";
        var amount = 0;
        if (store.Balance.TryGetValue(cpu, out FixedPoint2 val))
            amount = (int) val.Int();
        amount = Math.Clamp(amount, 0, 999);

        _sprite.LayerSetRsiState(args.SpriteViewEnt.AsNullable(), MalfAiHudVisualLayers.Digit1, $"{amount / 100 % 10}");
        _sprite.LayerSetRsiState(args.SpriteViewEnt.AsNullable(), MalfAiHudVisualLayers.Digit2, $"{amount / 10 % 10}");
        _sprite.LayerSetRsiState(args.SpriteViewEnt.AsNullable(), MalfAiHudVisualLayers.Digit3, $"{amount % 10}");
    }
}
