// SPDX-FileCopyrightText: 2022 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Whisper <121047731+QuietlyWhisper@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Content.Shared.Traits.Assorted;
using Content.Goobstation.Common.CCVar; // Goob
using Robust.Shared.Timing; // Goob
using Robust.Shared.Configuration; // Goob
using Robust.Shared.Prototypes;

namespace Content.Shared.Drunk;

public abstract class SharedDrunkSystem : EntitySystem
{
    public static readonly ProtoId<StatusEffectPrototype> DrunkKey = "Drunk";

    [Dependency] private readonly Content.Shared.StatusEffectNew.StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly SharedSlurredSystem _slurredSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!; // Goob - needed to calculate remaining status time. 
    [Dependency] private readonly IConfigurationManager _cfg = default!; // Goob - used to get the CVar setting. 

    public void TryApplyDrunkenness(EntityUid uid, float boozePower, bool applySlur = true,
        StatusEffectsComponent? status = null)
    {
        if (!Resolve(uid, ref status, false))
            return;

        if (TryComp<LightweightDrunkComponent>(uid, out var trait))
            boozePower *= trait.BoozeStrengthMultiplier;

        if (applySlur)
        {
            _slurredSystem.DoSlur(uid, TimeSpan.FromSeconds(boozePower), status);
        }

        if (!_statusEffectsSystem.HasStatusEffect(uid, DrunkKey.Id))
        {
            _statusEffectsSystem.TryAddStatusEffect(uid, DrunkKey.Id, out _, TimeSpan.FromSeconds(boozePower));
        }
        // Goob modification starts here
        else if (_statusEffectsSystem.TryGetTime(uid, DrunkKey.Id, out var time))
        {
            float maxDrunkTime = _cfg.GetCVar(GoobCVars.MaxDrunkTime);
            if (time.EndEffectTime == null)
                return;

            var timeLeft = (float) (time.EndEffectTime.Value - _timing.CurTime).TotalSeconds;

            if (timeLeft + boozePower > maxDrunkTime)
                _statusEffectsSystem.TrySetTime(uid, DrunkKey.Id, TimeSpan.FromSeconds(maxDrunkTime));
            else
                _statusEffectsSystem.TryAddTime(uid, DrunkKey.Id, TimeSpan.FromSeconds(boozePower));
        }
        // Goob modification ends
        // else
        // {
        //     _statusEffectsSystem.TryAddTime(uid, DrunkKey, TimeSpan.FromSeconds(boozePower), status);
        // }
    }

    public void TryRemoveDrunkenness(EntityUid uid)
    {
        _statusEffectsSystem.TryRemoveStatusEffect(uid, DrunkKey.Id);
    }
    public void TryRemoveDrunkenessTime(EntityUid uid, double timeRemoved)
    {
        _statusEffectsSystem.TryAddTime(uid, DrunkKey.Id, -TimeSpan.FromSeconds(timeRemoved));
    }

}
