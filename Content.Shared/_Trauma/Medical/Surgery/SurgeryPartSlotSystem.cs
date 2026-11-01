// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared._Shitmed.Medical.Surgery.Conditions;
using Content.Shared._Shitmed.Medical.Surgery.Steps;
using Content.Shared.Body.Systems;

namespace Content.Shared._Trauma.Medical.Surgery;

public sealed partial class SurgeryPartSlotSystem : EntitySystem
{
    [Dependency] private SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryPartSlotConditionComponent, SurgeryValidEvent>(OnSlotConditionValid);
        SubscribeLocalEvent<SurgeryAddPartSlotStepComponent, SurgeryStepEvent>(OnAddSlotStep);
        SubscribeLocalEvent<SurgeryAddPartSlotStepComponent, SurgeryStepCompleteCheckEvent>(OnAddSlotCheck);
    }

    private void OnSlotConditionValid(Entity<SurgeryPartSlotConditionComponent> ent, ref SurgeryValidEvent args)
    {
        args.Cancelled |= _body.CanAttachToSlot(args.Part, ent.Comp.Slot) == ent.Comp.Inverse;
    }

    private void OnAddSlotStep(Entity<SurgeryAddPartSlotStepComponent> ent, ref SurgeryStepEvent args)
    {
        if (!TryComp<SurgeryPartSlotConditionComponent>(args.Surgery, out var condition))
            return;

        _body.TryCreatePartSlot(args.Part, condition.Slot, condition.PartType, condition.Symmetry, out _);
    }

    private void OnAddSlotCheck(Entity<SurgeryAddPartSlotStepComponent> ent, ref SurgeryStepCompleteCheckEvent args)
    {
        if (!TryComp<SurgeryPartSlotConditionComponent>(args.Surgery, out var condition))
            return;

        args.Cancelled |= !_body.CanAttachToSlot(args.Part, condition.Slot);
    }
}
