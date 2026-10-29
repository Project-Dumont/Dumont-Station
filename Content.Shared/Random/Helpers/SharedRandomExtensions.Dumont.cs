// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.Random.Helpers;

// Dumont start
public static partial class SharedRandomExtensions
{
    public static int HashCodeCombine(int first, int second, params int[] remaining)
    {
        var values = new List<int> { first, second };
        values.AddRange(remaining);
        return HashCodeCombine(values);
    }

    public static IRobustRandom PredictedRandom(IGameTiming timing, NetEntity netEnt, NetEntity? netEnt2 = null)
    {
        var seed = HashCodeCombine((int) timing.CurTick.Value, netEnt.Id, netEnt2?.Id ?? 0);
        var random = new RobustRandom();
        random.SetSeed(seed);
        return random;
    }

    public static bool PredictedProb(IGameTiming timing, float probability, NetEntity netEnt1, NetEntity? netEnt2 = null)
        => PredictedRandom(timing, netEnt1, netEnt2).Prob(probability);
}
// Dumont end
