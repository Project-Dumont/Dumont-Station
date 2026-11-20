// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using BodySystem = Content.Shared.Body.Systems.SharedBodySystem;

using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Goobstation.Common.Magic;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Examine;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC;
using Content.Trauma.Shared.Heretic.Components.Ghoul;
using Content.Trauma.Shared.Heretic.Components.Side;
using Robust.Shared.Enums;
using Robust.Shared.Player;

using Content.Medical.Shared.Body;
// Dumont end

namespace Content.Trauma.Shared.Heretic.Systems;

public abstract partial class SharedGhoulSystem : EntitySystem
{
    [Dependency] protected ISharedPlayerManager Player = default!;

    [Dependency] private INetManager _net = default!;
    [Dependency] protected BodySystem Body = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private DamageableSystem _dmg = default!;

    [Dependency] private EntityQuery<WoundableComponent> _woundableQuery = default!;

    private static readonly ProtoId<DamageTypePrototype> Blunt = "Blunt";

    [SubscribeLocalEvent]
    private void OnExamine(Entity<GhoulComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.ExamineMessage != null)
            args.PushMarkup(Loc.GetString(ent.Comp.ExamineMessage));
    }

    [SubscribeLocalEvent]
    private void OnWeaponExamine(Entity<GhoulWeaponComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(ent.Comp.ExamineMessage));
    }

    public virtual void UnGhoulifyEntity(Entity<GhoulComponent> ent) { }

    [SubscribeLocalEvent]
    private void OnTryAttack(Entity<HereticMinionComponent> ent, ref AttackAttemptEvent args)
    {
        if (args.Target is not { } target)
            return;

        if (target == ent.Comp.BoundHeretic || HasComp<ShadowCloakEntityComponent>(target) &&
            Transform(target).ParentUid == ent.Comp.BoundHeretic)
            args.Cancel();
    }


    [SubscribeLocalEvent]
    private void OnBeforeMindSwap(Entity<GhoulComponent> ent, ref BeforeMindSwappedEvent args)
    {
        if (args.Cancelled)
            return;

        args.Cancelled = true;
        args.Message = "ghoul";
    }


    /// <summary>
    /// Required to prevent heretic from farming organs from ghouls
    /// </summary>
    public void MakeOrgansFragile(EntityUid uid)
    {
        foreach (var organ in Body.GetBodyOrgans(uid))
        {
            if (organ.Component.SlotId.Equals("brain", StringComparison.OrdinalIgnoreCase))
                continue;

            EnsureComp<FragileOrganComponent>(organ.Id);
        }
        // Dumont start
        foreach (var part in Body.GetBodyChildren(uid))
        {
            if (part.Component.PartType is not (Content.Shared.Body.Part.BodyPartType.Chest or Content.Shared.Body.Part.BodyPartType.Groin))
                EnsureComp<FragileOrganComponent>(part.Id);
        }
        // Dumont end
    }

    /// <summary>
    /// If ghoul is not npc controlled and not player controlled (SSD), this method kills and deconverts it
    /// Returns true on success or if entity is not a ghoul
    /// </summary>
    public bool TryKillAndDeconvertInactiveGhoul(Entity<GhoulComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return true;

        // Client check because client can't see other sessions, just assume they have non ssd mind
        if (_net.IsClient)
            return false;

        if (HasComp<ActiveNPCComponent>(ent))
            return false;

        // If body has session attached, don't touch it
        if (Player.TryGetSessionByEntity(ent, out var session) && session.Status == SessionStatus.InGame)
            return false;

        if (!_mobState.IsAlive(ent.Owner))
        {
            UnGhoulifyEntity(ent!);
            return true;
        }

        var dmg = new DamageSpecifier(ProtoMan.Index(Blunt), ent.Comp.TotalHealth * 1.2f);
        // Dumont start
        _dmg.ChangeDamage(ent.Owner, dmg, targetPart: TargetBodyPart.Vital, ignoreResistances: true);
        // Dumont end

        // Ghoul component should automatically be removed on death in most cases, or ghoul gets givved
        if (TerminatingOrDeleted(ent) || !Resolve(ent, ref ent.Comp, false))
            return true;

        UnGhoulifyEntity(ent!);
        return true;
    }
}
