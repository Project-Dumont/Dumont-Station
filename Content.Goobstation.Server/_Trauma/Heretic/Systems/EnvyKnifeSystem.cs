// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using Content.Server.Actions;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Weapons.Melee.Events;
using Content.Trauma.Shared.Heretic.Components.Side;
using Robust.Shared.Serialization.Manager;

namespace Content.Trauma.Server.Heretic.Systems;

public sealed partial class EnvyKnifeSystem : EntitySystem
{
    [Dependency] private SharedHumanoidAppearanceSystem _appearance = default!;
    [Dependency] private MetaDataSystem _metadata = default!;
    [Dependency] private SharedIdentitySystem _identity = default!;
    [Dependency] private ActionsSystem _actions = default!;
    [Dependency] private ISerializationManager _serialization = default!;

    [SubscribeLocalEvent]
    private void OnHit(Entity<EnvyKnifeComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        var victim = args.HitEntities[0];
        if (victim == args.User || HasComp<EnvyDisguiseStateComponent>(victim) ||
            !TryComp<HumanoidAppearanceComponent>(args.User, out var userAppearance) ||
            !TryComp<HumanoidAppearanceComponent>(victim, out var victimAppearance) ||
            Name(victim) == Name(args.User))
            return;

        var disguise = EnsureComp<EnvyDisguiseStateComponent>(args.User);
        disguise.OriginalAppearance ??= _serialization.CreateCopy(userAppearance, notNullableOverride: true);
        disguise.OriginalName ??= Name(args.User);
        _appearance.CloneAppearance(victim, args.User, victimAppearance, userAppearance);
        _metadata.SetEntityName(args.User, Name(victim), raiseEvents: false);
        _identity.QueueIdentityUpdate(args.User);

        if (disguise.RevertAction == null)
        {
            _actions.AddAction(args.User, ref disguise.RevertAction, ent.Comp.RevertAction);
            if (disguise.RevertAction is { } action)
                _actions.SetEntityIcon(action, args.User);
        }
    }

    [SubscribeLocalEvent]
    private void OnRevert(Entity<EnvyDisguiseStateComponent> ent, ref DisguiseRevertEvent args)
    {
        if (args.Handled || ent.Comp.OriginalAppearance is not { } original ||
            ent.Comp.OriginalName is not { } name || !HasComp<HumanoidAppearanceComponent>(ent))
            return;

        args.Handled = true;
        _appearance.CloneAppearance(ent, ent, original);
        _metadata.SetEntityName(ent, name, raiseEvents: args.RaiseRenameEvents);
        _identity.QueueIdentityUpdate(ent);
        _actions.RemoveAction(ent.Owner, ent.Comp.RevertAction);
        RemComp(ent, ent.Comp);
    }
}
// Dumont end
