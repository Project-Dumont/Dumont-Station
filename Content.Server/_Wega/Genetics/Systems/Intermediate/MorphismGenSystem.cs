using System.Linq;
using Content.Server.Humanoid;
using Content.Shared.Actions;
using Content.Shared.Genetics;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Player;

namespace Content.Server.Genetics;

public sealed class MorphismGenSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly MarkingManager _markings = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MorphismGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MorphismGenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MorphismGenComponent, MorphismGenActionEvent>(OnMorphismAction);

        SubscribeNetworkEvent<MorphismChangeMarkingEvent>(OnChangeMarking);
        SubscribeNetworkEvent<MorphismChangeMarkingColorEvent>(OnChangeMarkingColor);
        SubscribeNetworkEvent<MorphismSlotEvent>(OnSlotChange);
        SubscribeNetworkEvent<MorphismChangeBodyColorEvent>(OnBodyColorChange);
    }

    private void OnInit(Entity<MorphismGenComponent> ent, ref ComponentInit args)
        => ent.Comp.ActionEntity = _action.AddAction(ent, ent.Comp.ActionId);

    private void OnShutdown(Entity<MorphismGenComponent> ent, ref ComponentShutdown args)
        => _action.RemoveAction(ent.Comp.ActionEntity);

    private void OnMorphismAction(Entity<MorphismGenComponent> ent, ref MorphismGenActionEvent args)
    {
        if (args.Handled) return;
        args.Handled = true;
        SendState(args.Performer, args.Performer);
    }

    private void SendState(EntityUid target, EntityUid receiver)
    {
        if (!TryComp<HumanoidAppearanceComponent>(target, out var humanoid))
            return;

        var ev = new MorphismMenuOpenedEvent(
            GetNetEntity(target),
            humanoid.Species,
            humanoid.SkinColor,
            humanoid.EyeColor,
            humanoid.MarkingSet
        );

        RaiseNetworkEvent(ev, receiver);
    }

    private void OnChangeMarking(MorphismChangeMarkingEvent args, EntitySessionEventArgs sessionArgs)
    {
        var target = GetEntity(args.TargetUser);
        _humanoid.SetMarkingId(target, args.Category, args.Slot, args.MarkingId);
    }

    private void OnChangeMarkingColor(MorphismChangeMarkingColorEvent args, EntitySessionEventArgs sessionArgs)
    {
        var target = GetEntity(args.TargetUser);
        _humanoid.SetMarkingColor(target, args.Category, args.Slot, args.Colors);
    }

    private void OnSlotChange(MorphismSlotEvent args, EntitySessionEventArgs sessionArgs)
    {
        var target = GetEntity(args.TargetUser);

        if (args.Action == MorphismSlotEvent.SlotAction.Remove)
        {
            _humanoid.RemoveMarking(target, args.Category, args.SlotIndex);
        }
        else 
        {
            if (!TryComp<HumanoidAppearanceComponent>(target, out var humanoid)) return;

            var marking = _markings.MarkingsByCategory(args.Category).Keys.FirstOrDefault();
            
            if (!string.IsNullOrEmpty(marking))
            {
                _humanoid.AddMarking(target, marking, Color.Black);
            }
        }
        SendState(target, sessionArgs.SenderSession.AttachedEntity!.Value);
    }

    private void OnBodyColorChange(MorphismChangeBodyColorEvent args, EntitySessionEventArgs sessionArgs)
    {
        var target = GetEntity(args.TargetUser);
        if (!TryComp<HumanoidAppearanceComponent>(target, out var humanoid))
            return;

        if (args.Part == MorphismChangeBodyColorEvent.BodyPart.Skin)
        {
            humanoid.SkinColor = args.NewColor;
            Dirty(target, humanoid);

            var allBodyParts = Enum.GetValues<MarkingCategories>()
                .Where(c => c != MarkingCategories.Hair && 
                            c != MarkingCategories.FacialHair && 
                            c != MarkingCategories.Overlay &&
                            c != MarkingCategories.Undershirt &&
                            c != MarkingCategories.Underwear);

            foreach (var cat in allBodyParts)
            {
                if (humanoid.MarkingSet.TryGetCategory(cat, out var markings))
                {
                    for (int i = 0; i < markings.Count; i++)
                    {
                        var newColors = new List<Color>();
                        for(int j = 0; j < markings[i].MarkingColors.Count; j++) 
                        {
                            newColors.Add(args.NewColor);
                        }
                        _humanoid.SetMarkingColor(target, cat, i, newColors);
                    }
                }
            }
        }
        else
        {
            humanoid.EyeColor = args.NewColor;
            _humanoid.SetBaseLayerColor(target, HumanoidVisualLayers.Eyes, args.NewColor);
            Dirty(target, humanoid);
        }
    }
}
