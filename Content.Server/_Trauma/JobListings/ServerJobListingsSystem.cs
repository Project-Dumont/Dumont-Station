// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Random;
using Robust.Shared.Player;
using Robust.Server.GameStates;
using Content.Shared.Mind;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Random.Helpers;
using Content.Trauma.Common.Traitor;
using Content.Trauma.Shared.JobListings;
using System.Linq;

namespace Content.Trauma.Server.JobListings;

public sealed partial class ServerJobListingsSystem : JobListingsSystem
{
    [Dependency] private PvsOverrideSystem _pvsOverride = default!;
    [Dependency] private ISharedPlayerManager _player = default!;
    [Dependency] private IRobustRandom _random = default!;

    /// <summary>
    /// Assign the store owner a random side job.
    /// When the traitor is assigned their uplink, the traitor's mind becomes the store's owner.
    /// This has to be server-side because too many objective event handlers are server-side.
    /// </summary>
    public bool AssignSideJob(Entity<JobListingsComponent> jobBoard, int effectiveLevel)
    {
        if (jobBoard.Comp.Mind is not { } mind)
            return false;

        if (!MindQuery.TryComp(mind, out var mindComp))
            return false;

        var ev = new GenerateSideJobsEvent(effectiveLevel, (mind, mindComp), new(), new());
        RaiseLocalEvent(jobBoard, ref ev);
        if (ev.PrioritySideJobs.Count == 0 && ev.SideJobs.Count == 0)
            return false;

        var sideJob = _random.PickAndTake(ev.PrioritySideJobs.Count >= 1 ? ev.PrioritySideJobs : ev.SideJobs);
        var inserted = Container.Insert(sideJob, jobBoard.Comp.AvailableSideJobs);
        if (!inserted)
            Del(sideJob);

        foreach (var otherSideJob in ev.PrioritySideJobs)
        {
            Del(otherSideJob);
        }
        foreach (var otherSideJob in ev.SideJobs)
        {
            Del(otherSideJob);
        }

        return inserted;
    }

    /// <summary>
    /// Raise all the neccessary events to initialise the side job.
    /// </summary>
    /// <returns>Returns false if initialisation failed and the side job was deleted.</returns>
    public bool InitializeSideJob(EntityUid sideJob, Entity<MindComponent> mind, int effectiveLevel)
    {
        if (!ObjectiveQuery.TryComp(sideJob, out var objectiveComp))
        {
            Del(sideJob);
            return false;
        }

        var ev1 = new ObjectiveAssignedEvent(mind, mind.Comp);
        RaiseLocalEvent(sideJob, ref ev1);
        if (ev1.Cancelled)
        {
            Del(sideJob);
            return false;
        }

        var ev3 = new SideJobCreatedEvent(effectiveLevel);
        RaiseLocalEvent(sideJob, ref ev3);

        if (ev3.Cancelled || !SideJobQuery.TryComp(sideJob, out var sideJobComp) || sideJobComp.Reward is null)
        {
            Del(sideJob);
            return false;
        }

        var ev2 = new ObjectiveAfterAssignEvent(mind, mind.Comp, objectiveComp, MetaData(sideJob));
        RaiseLocalEvent(sideJob, ref ev2);

        return true;
    }

    /// <summary>
    /// Assign the traitor side jobs until their available slots are filled.
    /// </summary>
    public bool FillSideJobs(Entity<JobListingsComponent> jobBoard)
    {
        var effectiveLevel = GetReputationLevel(jobBoard);
        var jobsAssignedOfCurrentLevel = 0;

        while (CountSideJobs(jobBoard) < jobBoard.Comp.MaximumSideJobs)
        {
            if (!AssignSideJob(jobBoard, effectiveLevel))
            {
                // if we are above 0 effective level, try reduce it by 1 and try again to assign
                if (effectiveLevel > 0)
                {
                    jobsAssignedOfCurrentLevel = 0;
                    effectiveLevel -= 1;
                    continue;
                }

                return false;
            }

            // if we reached the limit then start assigning jobs of the level below
            // this is so that when we reach a new level we still get some jobs of the old level to keep things interesting
            jobsAssignedOfCurrentLevel += 1;
            if (jobsAssignedOfCurrentLevel >= jobBoard.Comp.SideJobsPerLevel && effectiveLevel > 0)
            {
                jobsAssignedOfCurrentLevel = 0;
                effectiveLevel -= 1;
            }
        }

        return true;
    }

    public override void Refresh(Entity<JobListingsComponent> jobBoard)
    {
        base.Refresh(jobBoard);
        Container.CleanContainer(jobBoard.Comp.AvailableSideJobs);
        FillSideJobs(jobBoard);
    }

    /// <summary>
    /// Helper method to add a PVS override for the job board.
    /// The job board / uplink store is a nullspace entity which would not normally be replicated.
    /// It is supposed to be shared between uplinks and persist if any of them are destroyed so it can't be put in an uplink's container.
    /// </summary>
    private void PVSOverrideEntity(Entity<MindComponent> mind, EntityUid entity)
    {
        if (mind.Comp.OwnedEntity is null)
            return;
        if (!_player.TryGetSessionByEntity(mind.Comp.OwnedEntity.Value, out var session))
            return;
        _pvsOverride.AddSessionOverride(entity, session);
    }

    /// <summary>
    /// Update the CachedProgress field on a sidejob.
    /// </summary>
    /// <param name="sideJob"></param>
    private void UpdateSideJob(Entity<JobListingsComponent> jobBoard, Entity<SideJobComponent> sideJob)
    {
        if (jobBoard.Comp.Mind is null)
            return;
        var mind = jobBoard.Comp.Mind;
        if (!MindQuery.TryComp(mind, out var mindComp))
            return;
        var progress = _objectives.GetProgress(sideJob.Owner, (mind.Value, mindComp));
        if (progress is null)
            return;
        sideJob.Comp.CachedProgress = progress.Value;
        DirtyField(sideJob.Owner, sideJob.Comp, nameof(SideJobComponent.CachedProgress));
    }

    protected override void UpdateAllSideJobs(Entity<JobListingsComponent> jobBoard)
    {
        foreach (var sideJob in jobBoard.Comp.AvailableSideJobs.ContainedEntities)
        {
            if (!SideJobQuery.TryComp(sideJob, out var sideJobComp))
                return;
            UpdateSideJob(jobBoard, (sideJob, sideJobComp));
        }

        foreach (var sideJob in jobBoard.Comp.AcceptedSideJobs.ContainedEntities)
        {
            if (!SideJobQuery.TryComp(sideJob, out var sideJobComp))
                return;
            UpdateSideJob(jobBoard, (sideJob, sideJobComp));
        }
    }

    [SubscribeLocalEvent]
    private void OnUplinkAssigned(ref UplinkAssignedEvent args)
    {
        RegisterUplink(args.Mind, args.Host);
    }

    private void RegisterUplink(EntityUid mind, EntityUid host)
    {
        var owner = EnsureComp<UplinkOwnerComponent>(mind);
        owner.Remotes.Add(host);
        if (TryComp<JobListingsOwnerComponent>(mind, out var jobs) &&
            !HasComp<RemoteJobListingsComponent>(host))
            Link((jobs.JobListings, JobListingsQuery.Comp(jobs.JobListings)), host);
    }

    [SubscribeLocalEvent]
    private void OnImplanted(Entity<SubdermalImplantComponent> implant, ref ImplantImplantedEvent args)
    {
        if (!HasComp<JobListingsImplantComponent>(implant) || args.Implanted is not {} body || Mind.GetMind(body) is not {} mind ||
            !HasComp<UplinkOwnerComponent>(mind))
            return;

        RegisterUplink(mind, implant);
        if (HasComp<JobListingsOwnerComponent>(mind))
            _implant.GrantAction((implant.Owner, Comp<JobListingsImplantComponent>(implant)), body);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<SubdermalImplantComponent> implant, ref ImplantRemovedEvent args)
    {
        if (GetJobBoard(implant) is {} board)
        {
            board.Comp.Remotes.Remove(implant);
            Dirty(board);
            RemComp<RemoteJobListingsComponent>(implant);
            Ui.CloseUi(implant.Owner, Content.Trauma.Common.JobListings.JobListingsUiKey.Key);
        }

        if (Mind.GetMind(args.Implanted) is {} mind && TryComp<UplinkOwnerComponent>(mind, out var owner))
            owner.Remotes.Remove(implant);
    }

    public bool TryBuyJobBoard(EntityUid buyer, EntityUid store, CreateJobBoardEvent product)
    {
        if (!Mind.TryGetMind(buyer, out var mind, out var mindComp) ||
            !TryComp<UplinkOwnerComponent>(mind, out var owner) || !owner.Remotes.Contains(store) ||
            HasComp<JobListingsOwnerComponent>(mind))
            return false;

        var uid = Spawn(product.JobBoardProto);
        var board = JobListingsQuery.Comp(uid);
        board.Mind = mind;
        Dirty(uid, board);
        PVSOverrideEntity((mind, mindComp), uid);
        AddComp(mind, new JobListingsOwnerComponent { JobListings = uid });
        FillSideJobs((uid, board));
        SetRefreshTime((uid, board));
        foreach (var remote in owner.Remotes)
        {
            if (!Exists(remote))
                continue;
            Link((uid, board), remote);
            if (HasComp<Content.Shared.PDA.PdaComponent>(remote))
                _pda.UpdatePdaUi(remote);
            if (TryComp<JobListingsImplantComponent>(remote, out var implant))
                _implant.GrantAction((remote, implant), buyer);
        }
        return true;
    }

    [Dependency] private Content.Server.PDA.PdaSystem _pda = default!;
    [Dependency] private JobListingsImplantSystem _implant = default!;
}
