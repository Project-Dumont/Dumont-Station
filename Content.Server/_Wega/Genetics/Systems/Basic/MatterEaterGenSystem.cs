using Content.Server.Nutrition.Components;
using Content.Server.Nutrition.EntitySystems;
using Content.Server.Popups;
using Content.Server.Stack;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Edible.Matter;
using Content.Shared.Genetics;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Stacks;
using Content.Shared.Storage;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Utility;

namespace Content.Server.Genetics.System;

public sealed class MatterEaterSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly FoodSystem _food = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EdibleMatterComponent, MatterEaterDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<EdibleMatterComponent, GetVerbsEvent<AlternativeVerb>>(AddMatterEatVerb);
    }

    public bool TryEatMatter(EntityUid user, EntityUid target, EntityUid matter, EdibleMatterComponent comp)
    {
        if (!comp.CanBeEaten || HasComp<FoodComponent>(matter)
            || !TryComp<PhysicsComponent>(matter, out var physics))
            return false;

        if (!HasComp<MatterEaterGenComponent>(user))
            return false;

        if (TryComp<MobStateComponent>(matter, out var mobState) && _mobState.IsAlive(matter, mobState))
            _mobState.ChangeMobState(matter, MobState.Dead, mobState);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            user,
            0.75f * physics.Mass,
            new MatterEaterDoAfterEvent(),
            matter,
            target,
            matter
        )
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        return true;
    }

    private void OnDoAfter(Entity<EdibleMatterComponent> ent, ref MatterEaterDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null
            || !TryComp<MatterEaterGenComponent>(args.User, out var eater))
            return;

        if (_food.IsMouthBlocked(args.Target.Value))
            return;

        if (!_interaction.InRangeUnobstructed(args.User, args.Target.Value))
            return;

        if (TryComp<StackComponent>(ent, out var stack) && stack.Count > 1)
            _stack.SetCount(ent.Owner, stack.Count - 1);
        else
        {
            if (TryComp<StorageComponent>(ent, out var storage))
                _container.EmptyContainer(storage.Container, destination: Transform(ent).Coordinates);

            QueueDel(ent);
        }

        if (TryComp<HungerComponent>(args.User, out var hunger))
            _hunger.ModifyHunger(args.User, ent.Comp.NutritionValue, hunger);

        _audio.PlayPvs(eater.EatSound, args.User);

        _popup.PopupEntity(Loc.GetString("matter-eater-succes", ("eat", Name(ent))), args.User);
        args.Handled = true;

        var forceFeed = args.User != args.Target;
        if (forceFeed)
            _adminLogger.Add(LogType.ForceFeed, LogImpact.Medium, $"{ToPrettyString(ent.Owner):user} forced {ToPrettyString(args.User):target} to eat {ToPrettyString(ent.Owner):food}");
        else
            _adminLogger.Add(LogType.Ingestion, LogImpact.Low, $"{ToPrettyString(args.User):target} ate {ToPrettyString(ent.Owner):food}");

        args.Repeat = !forceFeed && Exists(ent);
    }

    private void AddMatterEatVerb(Entity<EdibleMatterComponent> ent, ref GetVerbsEvent<AlternativeVerb> ev)
    {
        if (ent.Owner == ev.User || !ev.CanInteract || !ev.CanAccess || !ent.Comp.CanBeEaten
            || !HasComp<MatterEaterGenComponent>(ev.User) || HasComp<FoodComponent>(ent))
            return;

        if (TryComp<MobStateComponent>(ent, out var mobState)
            && _mobState.IsAlive(ent, mobState))
            return;

        var user = ev.User;
        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                TryEatMatter(user, user, ent, ent.Comp);
            },
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/cutlery.svg.192dpi.png")),
            Text = Loc.GetString("food-system-verb-eat"),
            Priority = -2
        };

        ev.Verbs.Add(verb);
    }
}
