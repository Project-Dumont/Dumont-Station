// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.Forensics.Components;

namespace Content.Trauma.Shared.Disease;

public sealed partial class DnaTargetDiseaseSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DiseaseCarrierComponent, DiseaseGainedEvent>(OnDiseaseGained);
        SubscribeLocalEvent<DiseaseDnaTargetConditionComponent, DiseaseCheckConditionsEvent>(OnCheckConditions);
    }

    private void OnDiseaseGained(Entity<DiseaseCarrierComponent> ent, ref DiseaseGainedEvent args)
    {
        if (!TryComp<DnaTargetDiseaseComponent>(args.Disease, out var target))
            return;

        target.Enabled = TryComp<DnaComponent>(ent, out var dna) && dna.DNA is {} value && target.TargetDnas.Contains(value);
        Dirty(args.Disease, target);
    }

    public void CopyTargets(EntityUid sourceDisease, EntityUid destination)
    {
        if (!TryComp<DnaTargetDiseaseComponent>(sourceDisease, out var source))
            return;

        var target = EnsureComp<DnaTargetDiseaseComponent>(destination);
        target.TargetDnas = new(source.TargetDnas);
        target.Enabled = false;
        Dirty(destination, target);
    }

    private void OnCheckConditions(Entity<DiseaseDnaTargetConditionComponent> ent, ref DiseaseCheckConditionsEvent args)
    {
        if (!TryComp<DnaTargetDiseaseComponent>(args.Disease, out var target) || !target.Enabled)
            args.DoEffect = false;
    }

    public void AddTargetDnas(EntityUid disease, HashSet<string> dnas)
    {
        var target = Comp<DnaTargetDiseaseComponent>(disease);
        target.TargetDnas.UnionWith(dnas);
        Dirty(disease, target);
    }
}
