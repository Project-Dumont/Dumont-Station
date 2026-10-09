using Content.Shared._Dumont.EscapePods; // Dumont
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._Starlight.Computers.PodConsole;

/// <summary>
/// Handles using the escape pod console to start a launch.
/// </summary>
public abstract partial class SharedPodConsoleSystem : EntitySystem
{

    [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private EscapePodCapacitySystem _capacity = default!; // Dumont
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PodConsoleComponent, ActivateInWorldEvent>(OnActivation);
        SubscribeLocalEvent<PodConsoleComponent, PodLaunchDoAfterEvent>(OnLaunch);
    }


    private void OnLaunch(Entity<PodConsoleComponent> ent, ref PodLaunchDoAfterEvent args)
    {
        if (args.Cancelled) return;
        // Dumont changes start
        if (CheckOvercrowded(ent, args.User))
            return;
        // Dumont end
        ent.Comp.Locked = true;
        ent.Comp.LaunchTime = _timing.CurTime + ent.Comp.LaunchDelay; // Dumont
        Dirty(ent); // Dumont
        _popup.PopupPredicted(Loc.GetString("pod-launching", ("time", (int) ent.Comp.LaunchDelay.TotalSeconds)), ent, args.User, PopupType.LargeCaution); // Dumont
        OnLaunchCountdown(ent); // Dumont
    }

    private void OnActivation(Entity<PodConsoleComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex) return;

        if (ent.Comp.Locked)
        {
            _popup.PopupClient(Loc.GetString("pod-locked"), ent, args.User);
            return;
        }

        // Dumont changes start
        if (CheckOvercrowded(ent, args.User))
        {
            args.Handled = true;
            return;
        }
        // Dumont end

        args.Handled = _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User,
            TimeSpan.FromSeconds(10), new PodLaunchDoAfterEvent(), ent, ent, ent)
        {
            BreakOnDamage = true,
            BlockDuplicate = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            MovementThreshold = 1f
        });
    }

    // Dumont changes start
    /// <summary>
    /// Called when a launch countdown starts on the console.
    /// </summary>
    protected virtual void OnLaunchCountdown(Entity<PodConsoleComponent> ent)
    {
    }

    /// <summary>
    /// Returns true and shows a popup when the pod has more people than it can carry.
    /// </summary>
    public bool CheckOvercrowded(Entity<PodConsoleComponent> ent, EntityUid? user)
    {
        if (!_capacity.IsOvercrowded(ent, out var occupants, out var max))
            return false;

        _popup.PopupPredicted(Loc.GetString("escape-pod-overcrowded", ("count", occupants), ("max", max)), ent, user, PopupType.MediumCaution);
        return true;
    }
    // Dumont end
}

[Serializable, NetSerializable]
public sealed partial class PodLaunchDoAfterEvent : SimpleDoAfterEvent
{

}
