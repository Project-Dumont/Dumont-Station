// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Flammability;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Trauma.Shared.Heretic.Systems;
using Content.Shared.Atmos;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Jittering;
using Content.Shared.StatusEffectNew;
using Content.Shared.Temperature;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Slasher.Systems;

public sealed class SlasherMadnessMaskSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedHereticSystem _heretic = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherMadnessMaskComponent, BeingUnequippedAttemptEvent>(OnUnequip);
        SubscribeLocalEvent<SlasherMadnessMaskComponent, InventoryRelayedEvent<GetFireProtectionEvent>>(OnGetProtection);
        SubscribeLocalEvent<SlasherMadnessMaskComponent, InventoryRelayedEvent<ModifyChangedTemperatureEvent>>(
            OnTemperatureChangeAttempt);
    }

    private void OnUnequip(Entity<SlasherMadnessMaskComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        if (_heretic.IsHereticOrGhoul(args.Unequipee))
            return;

        if (TryComp<ClothingComponent>(ent, out var clothing) && (clothing.Slots & args.SlotFlags) == SlotFlags.NONE)
            return;

        args.Cancel();
    }

    private void OnTemperatureChangeAttempt(Entity<SlasherMadnessMaskComponent> ent,
        ref InventoryRelayedEvent<ModifyChangedTemperatureEvent> args)
    {
        if (!_heretic.IsHereticOrGhoul(args.Args.Target))
            return;

        if (args.Args.TemperatureDelta > 0)
            args.Args.TemperatureDelta = 0;
    }

    private void OnGetProtection(Entity<SlasherMadnessMaskComponent> ent, ref InventoryRelayedEvent<GetFireProtectionEvent> args)
    {
        if (!_heretic.IsHereticOrGhoul(args.Args.Target) || HasComp<VeryFlammableComponent>(args.Args.Target))
            return;

        args.Args.Multiplier = -10f; // Basically ignore fire AP
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlasherMadnessMaskComponent, ClothingComponent>();
        while (query.MoveNext(out var uid, out var mask, out var clothing))
        {
            if (clothing.InSlot == null)
                continue;

            mask.UpdateAccumulator += frameTime;

            if (mask.UpdateAccumulator < mask.UpdateTimer)
                continue;

            mask.UpdateAccumulator = 0;

            var lookup = _lookup.GetEntitiesInRange(uid, 5f);
            foreach (var look in lookup)
            {
                // heathens exclusive
                if (_heretic.IsHereticOrGhoul(look))
                    continue;

                if (!mask.AffectWearer
                    && _inventory.TryGetContainingEntity(uid, out var wearer)
                    && look == wearer)
                    continue;

                if (HasComp<StaminaComponent>(look) && _random.Prob(mask.StaminaProb))
                    _stamina.TakeOvertimeStaminaDamage(look, mask.StaminaDamage);

                if (_random.Prob(mask.JitterProb))
                    _jitter.DoJitter(look, TimeSpan.FromSeconds(.5f), true, amplitude: 5, frequency: 10);

                if (_random.Prob(mask.RainbowProb))
                {
                    _statusEffect.TryAddStatusEffectDuration(look,
                        "StatusEffectSeeingRainbow",
                        out _,
                        mask.RainbowDuration);
                }
            }
        }
    }
}
