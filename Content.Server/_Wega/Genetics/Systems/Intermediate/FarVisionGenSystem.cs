using Content.Server.Movement.Components;
using Content.Shared.Actions;
using Content.Shared.Genetics;
using Robust.Shared.GameStates;

namespace Content.Server.Genetics;

public sealed class FarVisionGenSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FarVisionGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FarVisionGenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FarVisionGenComponent, FarVisionGenActionEvent>(OnToggle);
    }

    private void OnInit(Entity<FarVisionGenComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ActionEntity = _actions.AddAction(ent, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<FarVisionGenComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.ActionEntity != null)
            _actions.RemoveAction(ent.Comp.ActionEntity);

        RemComp<EyeCursorOffsetComponent>(ent);
    }

    private void OnToggle(Entity<FarVisionGenComponent> ent, ref FarVisionGenActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (TryComp<EyeCursorOffsetComponent>(ent, out var eyeComp))
        {
            RemComp<EyeCursorOffsetComponent>(ent);
            _actions.SetToggled(ent.Comp.ActionEntity, false);
        }
        else
        {
            EnsureComp<EyeCursorOffsetComponent>(ent).MaxOffset = ent.Comp.MaxOffset;
            EnsureComp<EyeCursorOffsetComponent>(ent).OffsetSpeed = ent.Comp.OffsetSpeed;
            EnsureComp<EyeCursorOffsetComponent>(ent).PvsIncrease = ent.Comp.PvsIncrease;
            Dirty(ent, EnsureComp<EyeCursorOffsetComponent>(ent)); 

            _actions.SetToggled(ent.Comp.ActionEntity, true);
        }
    }
}
