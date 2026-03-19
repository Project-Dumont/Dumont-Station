// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2023 KP <13428215+nok-ko@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Adventure.Bartender.Systems; // Adventure
using Content.Server.Damage.Components;
using Content.Shared.Damage;
using Content.Shared.Throwing;
using Content.Shared.Chemistry.EntitySystems; // Omu - Make Beer Goggles Cool Again (MBGCA)

namespace Content.Server.Damage.Systems
{
    /// <summary>
    /// Damages the thrown item when it lands.
    /// </summary>
    public sealed class DamageOnLandSystem : EntitySystem
    {
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SpillProofThrowerSystem _nonspillthrower = default!; // Adventure
        [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!; // Omu (MBGCA)

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<DamageOnLandComponent, LandEvent>(DamageOnLand);
        }

        private void DamageOnLand(EntityUid uid, DamageOnLandComponent component, ref LandEvent args)
        {
            // Adventure start: Drinks thrown while wearing beer goggles do not take damage
            if (args.User is { } user
                && _nonspillthrower.GetSpillProofThrow(user)
                && _solutions.TryGetSolution(uid, "drink", out _))
            {
                return;
            }
            // Adventure end
            _damageableSystem.TryChangeDamage(uid, component.Damage, component.IgnoreResistances);
        }
    }
}
