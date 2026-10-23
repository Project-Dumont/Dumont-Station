// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.EntityEffects;
using Content.Trauma.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Trauma.Server.EntityEffects;

public sealed partial class ScrambleAccessReaderEffectSystem : EntitySystem
{
    [Dependency] private AccessReaderSystem _access = default!;
    [Dependency] private IRobustRandom _random = default!;

    private List<ProtoId<AccessLevelPrototype>> _allLevels = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteEntityEffectEvent<ScrambleAccessReader>>(OnEffect);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
        LoadPrototypes();
    }

    private void OnEffect(ref ExecuteEntityEffectEvent<ScrambleAccessReader> args)
    {
        // for airlocks modify the electroncs instead of the door directly
        if (!HasComp<AccessReaderComponent>(args.Args.TargetEntity) ||
            !_access.GetMainAccessReader(args.Args.TargetEntity, out var reader))
            return;

        _access.ClearAccesses(reader.Value);
        var count = _random.Next(args.Effect.Min, args.Effect.Max);
        for (int i = 0; i < count; i++)
        {
            AddRandomAccess(reader.Value);
        }
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<AccessLevelPrototype>())
            LoadPrototypes();
    }

    private void LoadPrototypes()
    {
        _allLevels.Clear();
        foreach (var level in ProtoMan.EnumeratePrototypes<AccessLevelPrototype>())
        {
            if (level.CanAddToIdCard)
                _allLevels.Add(level.ID);
        }
    }

    public void AddRandomAccess(Entity<AccessReaderComponent> ent)
    {
        var level = _random.Pick(_allLevels);
        _access.AddAccess(ent, level);
    }
}
