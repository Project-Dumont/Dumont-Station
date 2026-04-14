// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Organ;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Timing;

namespace Content.Server._Shitmed.Body.Organ;

public sealed class StatusEffectOrganSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StatusEffectsSystem _effects = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StatusEffectOrganComponent, OrganComponent>();
        var now = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp, out var organ))
        {
            if (now < comp.NextUpdate || organ.Body is not {} body)
                continue;

            comp.NextUpdate = now + comp.Delay;
            foreach (var (key, _) in comp.Refresh)
            {
                _effects.TryUpdateStatusEffectDuration(body, key.Id, comp.Delay);
            }
        }
    }
}
