// SPDX-FileCopyrightText: 2026 Guilherme Galinha Azul <guilhermegalinhaazul@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mutatrix.Components;
using Content.Shared.Mutatrix.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Mutatrix.Systems;

public abstract class SharedMutatrixSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private static readonly HashSet<ProtoId<MutatrixTransformationPrototype>> RemovedBuiltIns = new()
    {
        new("MutatrixRat"),
        new("MutatrixIPC"),
        new("MutatrixPlasmaman"),
        new("MutatrixRevenant"),
        new("MutatrixChitinid"),
        new("MutatrixGhoulStalker"),
        new("MutatrixGosma"),
        new("MutatrixFeroxi"),
        new("MutatrixArachnid"),
        new("MutatrixBaseMobAsteroid"),
        new("MutatrixBesta"),
        new("MutatrixChama"),
        new("MutatrixQuatroBracos"),
        new("MutatrixGreyMatter"),
    };

    private static bool IsRemovedBuiltIn(ProtoId<MutatrixTransformationPrototype> transformation)
    {
        return RemovedBuiltIns.Contains(transformation);
    }

    /// <summary>
    /// Adds all default-unlocked transformations to a DNA component.
    /// Safe to call more than once.
    /// Also prunes the removed built-in rat form left over from older Mutatrix patches.
    /// Dynamic scanned mice/rats are allowed again; they just are not default forms.
    /// </summary>
    public void EnsureDefaultUnlocks(Entity<MutatrixDnaComponent> ent)
    {
        var changed = PruneRemovedBuiltIn(ent.Comp);

        foreach (var transformation in _prototype.EnumeratePrototypes<MutatrixTransformationPrototype>())
        {
            // Segurança extra: mesmo que algum arquivo velho ainda defina MutatrixRat,
            // ele nunca volta para a roda inicial.
            if (IsRemovedBuiltIn(transformation.ID))
                continue;

            if (!transformation.DefaultUnlocked)
                continue;

            if (ent.Comp.Unlocked.Add(transformation.ID))
                changed = true;
        }

        if (changed)
            Dirty(ent);
    }

    private static bool PruneRemovedBuiltIn(MutatrixDnaComponent dna)
    {
        var changed = false;

        foreach (var removed in RemovedBuiltIns)
        {
            if (dna.Unlocked.Remove(removed))
                changed = true;

            if (dna.RoundUnlocked.Remove(removed))
                changed = true;
        }

        if (dna.Selected != null && IsRemovedBuiltIn(dna.Selected.Value))
        {
            dna.Selected = null;
            changed = true;
        }

        return changed;
    }

    public bool IsUnlocked(
        MutatrixDnaComponent dna,
        ProtoId<MutatrixTransformationPrototype> transformation)
    {
        // Bloqueia formas removidas da roda inicial.
        if (IsRemovedBuiltIn(transformation))
            return false;

        return dna.Unlocked.Contains(transformation)
            || dna.RoundUnlocked.Contains(transformation);
    }

    public bool IsDynamicUnlocked(MutatrixDnaComponent dna, string entityPrototype)
    {
        return dna.RoundScannedPrototypes.Contains(entityPrototype);
    }

    public HashSet<ProtoId<MutatrixTransformationPrototype>> GetAllUnlocked(MutatrixDnaComponent dna)
    {
        var result = new HashSet<ProtoId<MutatrixTransformationPrototype>>(dna.Unlocked);
        foreach (var round in dna.RoundUnlocked)
            result.Add(round);
        foreach (var removed in RemovedBuiltIns)
            result.Remove(removed);
        return result;
    }

    public HashSet<string> GetAllDynamicUnlocked(MutatrixDnaComponent dna)
    {
        // Agora não filtramos MobMouse/MobRat aqui: se foi escaneado no round, pode aparecer.
        return new HashSet<string>(dna.RoundScannedPrototypes);
    }
}
