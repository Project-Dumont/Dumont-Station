// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Coordinates;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Rotation;
using Content.Shared.Standing;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Components;

namespace Content.Server.WhiteDream.BloodCult.LeaderForm;

public sealed class BloodCultLeaderFormSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PentagramComponent, ComponentStartup>(OnPentagramStartup);
        SubscribeLocalEvent<PentagramComponent, ComponentShutdown>(OnPentagramShutdown);
        SubscribeLocalEvent<BloodCultLeaderComponent, ComponentStartup>(OnLeaderStartup);
        SubscribeLocalEvent<BloodCultLeaderComponent, ComponentShutdown>(OnLeaderShutdown);

        SubscribeLocalEvent<BloodCultLeaderFormComponent, ComponentShutdown>(OnFormShutdown);
        SubscribeLocalEvent<BloodCultLeaderFormComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<BloodCultLeaderFormComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<BloodCultLeaderFormComponent, StoodEvent>(OnStood);
    }

    private void OnPentagramStartup(Entity<PentagramComponent> ent, ref ComponentStartup args) => Refresh(ent);

    private void OnPentagramShutdown(Entity<PentagramComponent> ent, ref ComponentShutdown args) =>
        RemComp<BloodCultLeaderFormComponent>(ent);

    private void OnLeaderStartup(Entity<BloodCultLeaderComponent> ent, ref ComponentStartup args) => Refresh(ent);

    private void OnLeaderShutdown(Entity<BloodCultLeaderComponent> ent, ref ComponentShutdown args) =>
        RemComp<BloodCultLeaderFormComponent>(ent);

    private void Refresh(EntityUid uid)
    {
        if (TerminatingOrDeleted(uid) || HasComp<BloodCultLeaderFormComponent>(uid))
            return;

        if (!HasComp<BloodCultLeaderComponent>(uid) || !HasComp<PentagramComponent>(uid))
            return;

        var comp = AddComp<BloodCultLeaderFormComponent>(uid);
        var form = SpawnAttachedTo(comp.Form, uid.ToCoordinates());
        comp.FormEntity = form;

        var relay = EnsureComp<TargetInteractionRelayComponent>(form);
        relay.RelayEntity = uid;
        Dirty(form, relay);

        SetRotation(uid, comp);
        SetMobState(uid, comp);
    }

    private void OnFormShutdown(Entity<BloodCultLeaderFormComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.FormEntity is { } form)
            QueueDel(form);

        ent.Comp.FormEntity = null;
    }

    private void OnMobStateChanged(Entity<BloodCultLeaderFormComponent> ent, ref MobStateChangedEvent args) =>
        SetMobState(ent, ent.Comp);

    private void OnDowned(Entity<BloodCultLeaderFormComponent> ent, ref DownedEvent args) =>
        SetRotation(ent, ent.Comp);

    private void OnStood(Entity<BloodCultLeaderFormComponent> ent, ref StoodEvent args) =>
        SetRotation(ent, ent.Comp);

    private void SetRotation(EntityUid uid, BloodCultLeaderFormComponent comp)
    {
        if (comp.FormEntity is not { } form)
            return;

        _appearance.SetData(form,
            RotationVisuals.RotationState,
            _standing.IsDown(uid) ? RotationState.Horizontal : RotationState.Vertical);
    }

    private void SetMobState(EntityUid uid, BloodCultLeaderFormComponent comp)
    {
        if (comp.FormEntity is not { } form || !TryComp<MobStateComponent>(uid, out var mobState))
            return;

        _appearance.SetData(form, MobStateVisuals.State, mobState.CurrentState);
    }
}
