// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Genetics;
using Content.Shared.Movement.Components;
using Robust.Shared.Audio;

namespace Content.Server.Genetics.System;

public sealed class JumpGenSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<JumpGenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<JumpGenComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<JumpGenComponent> ent, ref ComponentInit args)
    {
        var jump = EnsureComp<JumpAbilityComponent>(ent);

        ent.Comp.ActionEntity = _action.AddAction(ent, ent.Comp.ActionId);
        jump.JumpDistance = 10f; 
        jump.JumpSound = new SoundPathSpecifier("/Audio/Voice/Reptilian/reptilian_tailthump.ogg");
        Dirty(ent, jump);
    }

    private void OnShutdown(Entity<JumpGenComponent> ent, ref ComponentShutdown args)
    {
        if (HasComp<JumpAbilityComponent>(ent))
            RemComp<JumpAbilityComponent>(ent);

        _action.RemoveAction(ent.Comp.ActionEntity);
    }
}
