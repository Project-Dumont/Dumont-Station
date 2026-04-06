// SPDX-FileCopyrightText: 2026 Princess Cheeseballs <66055347+Princess-Cheeseballs@users.noreply.github.com>
// SPDX-License-Identifier: MIT

using Content.Shared.Silicons.StationAi;

namespace Content.Shared.Mind.Filters;

public sealed partial class AliveAiPool : IMindPool
{
    void IMindPool.FindMinds(HashSet<Entity<MindComponent>> minds, EntityUid? exclude, IEntityManager entMan, SharedMindSystem mindSys)
    {
        var query = entMan.EntityQueryEnumerator<StationAiHeldComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (mindSys.TryGetMind(uid, out var mind, out var comp) && mind != exclude && !mindSys.IsCharacterDeadIc(comp))
                minds.Add((mind, comp));
        }
    }
}
