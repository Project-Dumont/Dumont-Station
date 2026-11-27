using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Ninja.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._Dumont.EscapePods;

/// <summary>
/// Handles ninjas hacking escape pod consoles to leave the station.
/// </summary>
public sealed partial class SharedPodHackerSystem : EntitySystem
{
    [Dependency] private SharedNinjaGlovesSystem _gloves = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedPodConsoleSystem _podConsole = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PodHackerComponent, BeforeInteractHandEvent>(OnBeforeInteractHand);
        SubscribeLocalEvent<PodHackerComponent, ExtractDoAfterEvent>(OnDoAfter);
    }

    private void OnBeforeInteractHand(EntityUid uid, PodHackerComponent comp, BeforeInteractHandEvent args)
    {
        if (args.Handled || !HasComp<PodConsoleComponent>(args.Target))
            return;

        if (!_gloves.AbilityCheck(uid, args, out var target))
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, uid, comp.Delay, new ExtractDoAfterEvent(), target: target, used: uid, eventTarget: uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.5f,
            CancelDuplicate = false
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, PodHackerComponent comp, ExtractDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
            return;

        if (!TryComp<PodConsoleComponent>(target, out var console) || console.LaunchTime != null)
            return;

        if (_podConsole.CheckOvercrowded((target, console), uid))
            return;

        console.Locked = true;
        console.LaunchTime = _timing.CurTime;
        Dirty(target, console);

        var ev = new PodCalledInEvent(uid, target);
        RaiseLocalEvent(args.User, ref ev);
    }
}

/// <summary>
/// DoAfter event for hacking a pod console.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class ExtractDoAfterEvent : SimpleDoAfterEvent;

/// <summary>
/// Raised on the user when they hack a pod console and the pod is about to leave.
/// </summary>
[ByRefEvent]
public record struct PodCalledInEvent(EntityUid Used, EntityUid Target);
