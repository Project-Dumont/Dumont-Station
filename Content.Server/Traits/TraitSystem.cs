// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 forkeyboards <91704530+forkeyboards@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._EinsteinEngines.Language;
using Content.Shared.GameTicking;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Traits;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedHandsSystem _sharedHandsSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    // When the player is spawned in, add all trait components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        // Check if player's job allows to apply traits
        if (args.JobId == null ||
            !_prototypeManager.TryIndex<JobPrototype>(args.JobId ?? string.Empty, out var protoJob) ||
            !protoJob.ApplyTraits)
        {
            return;
        }

        ApplyTraits(args.Mob, args.Profile); // Orion-Edit
    }

    // Orion-Edit-Start
    public void ApplyTraits(EntityUid mob, HumanoidCharacterProfile profile)
    {
        foreach (var traitId in profile.TraitPreferences)
        {
            if (!_prototypeManager.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
            {
                Log.Warning($"No trait found with ID {traitId}!");
                continue;
            }

            if (_whitelistSystem.IsWhitelistFail(traitPrototype.Whitelist, mob) ||
                _whitelistSystem.IsBlacklistPass(traitPrototype.Blacklist, mob))
                continue;

            // Begin Goobstation: Species trait support
            if (traitPrototype.IncludedSpecies.Count > 0 && !traitPrototype.IncludedSpecies.Contains(profile.Species) ||
                traitPrototype.ExcludedSpecies.Contains(profile.Species))
                continue;
            // End Goobstation: Species trait support

            // Add all components required by the prototype
            EntityManager.AddComponents(mob, traitPrototype.Components, false);

            // Einstein Engines - Language begin (remove this if trait system refactor)
            // Remove/Add Languages required by the prototype
            var language = EntityManager.System<LanguageSystem>();

            if (traitPrototype.RemoveLanguagesSpoken is not null)
            {
                foreach (var lang in traitPrototype.RemoveLanguagesSpoken)
                {
                    language.RemoveLanguage(mob, lang, true, false);
                }
            }

            if (traitPrototype.RemoveLanguagesUnderstood is not null)
            {
                foreach (var lang in traitPrototype.RemoveLanguagesUnderstood)
                {
                    language.RemoveLanguage(mob, lang, false);
                }
            }

            if (traitPrototype.LanguagesSpoken is not null)
            {
                foreach (var lang in traitPrototype.LanguagesSpoken)
                {
                    language.AddLanguage(mob, lang, true, false);
                }
            }

            if (traitPrototype.LanguagesUnderstood is not null)
            {
                foreach (var lang in traitPrototype.LanguagesUnderstood)
                {
                    language.AddLanguage(mob, lang, false);
                }
            }
            // Einstein Engines - Language end

            // Add item required by the trait
            if (traitPrototype.TraitGear == null)
                continue;

            if (!TryComp(mob, out HandsComponent? handsComponent))
                continue;

            var coords = Transform(mob).Coordinates;
            var inhandEntity = Spawn(traitPrototype.TraitGear, coords);
            _sharedHandsSystem.TryPickup(mob,
                inhandEntity,
                checkActionBlocker: false,
                handsComp: handsComponent);
        }
    }
    // Orion-Edit-End
}
