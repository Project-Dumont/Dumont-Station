// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Numerics;
using Content.Shared.Research.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Goobstation.Research;

[TestFixture]
public sealed class TechnologyPrototypePositionTests
{
    [Test]
    public async Task TechnologyPrototypePositionsAreUniqueTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var protoManager = server.ResolveDependency<IPrototypeManager>();

        var fails = new List<string>();
        var positions = new HashSet<Vector2>();

        await server.WaitAssertion(() =>
        {
            foreach (var techProto in protoManager.EnumeratePrototypes<TechnologyPrototype>())
            {
                Vector2 position = techProto.Position;

                if (!positions.Add(position))
                    fails.Add($"ID: {techProto.ID} Position - {position}");
            }
        });

        if (fails.Count > 0)
        {
            var msg = string.Join("\n", fails) + "\n" + "Found duplicate positions for following" + nameof(TechnologyPrototype);
            Assert.Fail(msg);
        }

        await pair.CleanReturnAsync();
    }
}
