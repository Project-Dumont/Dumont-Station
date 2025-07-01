using System.Linq;
using Content.Server.Humanoid;
using Content.Shared.Corvax.TTS;
using Content.Shared.DetailExaminable;
using Content.Shared.Forensics.Components;
using Content.Shared.Genetics;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Inventory;
using Content.Shared.Speech.Synthesis.Components;
using Content.Shared.Wagging;

namespace Content.Server.Genetics.System;

public sealed partial class DnaModifierSystem
{
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;

    public bool TryCloneHumanoid(Entity<DnaModifierComponent> entity, Entity<DnaModifierComponent> target)
    {
        if (target.Comp.UniqueIdentifiers == null)
            return false;

        CloneHumanoid(entity, target);

        return true;
    }

    private void CloneHumanoid(Entity<DnaModifierComponent> entity, Entity<DnaModifierComponent> target,
        HumanoidAppearanceComponent? humanoid = null, HumanoidAppearanceComponent? targetHumanoid = null)
    {
        if (!Resolve(entity, ref humanoid) || !Resolve(target, ref targetHumanoid))
            return;

        if (target.Comp.UniqueIdentifiers == null)
            return;

        EnsureComp<DnaClonedComponent>(entity);

        humanoid.Species = targetHumanoid.Species;
        entity.Comp.UniqueIdentifiers = CloneUniqueIdentifiers(target.Comp.UniqueIdentifiers);
        if (TryComp<DetailExaminableComponent>(entity, out var detail))
        {
            detail.Content = "";
            if (TryComp<DetailExaminableComponent>(target, out var targetDetail))
                detail.Content = targetDetail.Content;
        }

        _metaData.SetEntityName(entity, Name(target));
        if (TryComp<DnaComponent>(entity, out var dna) && TryComp<DnaComponent>(target, out var targetDna))
            dna.DNA = targetDna.DNA;

        if (TryComp<TTSComponent>(entity, out var tts) && TryComp<TTSComponent>(target, out var targetTts))
            tts.VoicePrototypeId = targetTts.VoicePrototypeId;

        if (TryComp<SpeechSynthesisComponent>(entity, out var barks) && TryComp<SpeechSynthesisComponent>(target, out var targetBarks))
            barks.VoicePrototypeId = targetBarks.VoicePrototypeId;

        if (TryComp<InventoryComponent>(entity, out var inventory) && TryComp<InventoryComponent>(target, out var targetInventory))
        {
            _inventory.CloneInventory((entity, inventory), targetInventory);
            Dirty(entity, inventory);
        }

        if (HasComp<WaggingComponent>(target))
            EnsureComp<WaggingComponent>(entity);
        else
            RemComp<WaggingComponent>(entity);

        entity.Comp.UniqueIdentifiers!.Gender = target.Comp.UniqueIdentifiers!.Gender;

        // Nose cloning
        if (targetHumanoid.MarkingSet.TryGetCategory(MarkingCategories.Snout, out var snoutMarkings))
            _humanoid.AddMarking(entity, snoutMarkings.First().MarkingId, targetHumanoid.SkinColor);
        else
            humanoid.MarkingSet.RemoveCategory(MarkingCategories.Snout);

        Dirty(entity, entity.Comp);
        TryChangeUniqueIdentifiers(entity);
    }
}
