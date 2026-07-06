// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Eye.Blinding.Components;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.Eye.Blinding.Systems;

public sealed class TemporaryBlindnessSystem : EntitySystem
{
    public static readonly ProtoId<StatusEffectPrototype> BlindingStatusEffect = "TemporaryBlindness";

    [Dependency] private readonly BlindableSystem _blindableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TemporaryBlindnessComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TemporaryBlindnessComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TemporaryBlindnessComponent, CanSeeAttemptEvent>(OnBlindTrySee);
        // Orion-Start
        SubscribeLocalEvent<TemporaryBlindnessComponent, StatusEffectAppliedEvent>(OnStatusEffectApplied);
        SubscribeLocalEvent<TemporaryBlindnessComponent, StatusEffectRemovedEvent>(OnStatusEffectRemoved);
        SubscribeLocalEvent<TemporaryBlindnessComponent, StatusEffectRelayedEvent<CanSeeAttemptEvent>>(OnBlindTrySeeRelayed);
        // Orion-End
    }

    private void OnStartup(EntityUid uid, TemporaryBlindnessComponent component, ComponentStartup args)
    {
        // Orion-Edit-Start
        if (!TryComp<StatusEffectComponent>(uid, out var status) || status.AppliedTo == null)
            return;

        _blindableSystem.UpdateIsBlind(status.AppliedTo.Value);
        // Orion-Edit-End
    }

    private void OnShutdown(EntityUid uid, TemporaryBlindnessComponent component, ComponentShutdown args)
    {
        // Orion-Edit-Start
        if (!TryComp<StatusEffectComponent>(uid, out var status) || status.AppliedTo == null)
            return;

        _blindableSystem.UpdateIsBlind(status.AppliedTo.Value);
        // Orion-Edit-End
    }

    private static void OnBlindTrySee(EntityUid uid, TemporaryBlindnessComponent component, CanSeeAttemptEvent args) // Orion-Edit: Static
    {
        if (component.LifeStage <= ComponentLifeStage.Running)
            args.Cancel();
    }

    // Orion-Start
    private void OnStatusEffectApplied(Entity<TemporaryBlindnessComponent> ent, ref StatusEffectAppliedEvent args)
    {
        _blindableSystem.UpdateIsBlind(args.Target);
    }

    private void OnStatusEffectRemoved(Entity<TemporaryBlindnessComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _blindableSystem.UpdateIsBlind(args.Target);
    }

    private static void OnBlindTrySeeRelayed(Entity<TemporaryBlindnessComponent> ent, ref StatusEffectRelayedEvent<CanSeeAttemptEvent> args)
    {
        if (ent.Comp.LifeStage <= ComponentLifeStage.Running)
            args.Args.Cancel();
    }
    // Orion-End
}
