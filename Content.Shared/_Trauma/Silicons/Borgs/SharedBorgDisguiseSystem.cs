// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

using System.Diagnostics.CodeAnalysis;


using Content.Shared.Actions;
using Content.Trauma.Shared.Silicons.Borgs.Components;

namespace Content.Trauma.Shared.Silicons.Borgs;

/// <summary>
/// Manages Borg disguises, such as the Syndicate Saboteur's chameleon projector.
/// </summary>
public abstract partial class SharedBorgDisguiseSystem : EntitySystem
{
    [Dependency] protected SharedActionsSystem _actions = default!;
    [Dependency] private MetaDataSystem _meta = default!;
    [Dependency] protected SharedPointLightSystem _light = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgDisguiseComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BorgDisguiseComponent, ComponentShutdown>(OnCompRemove);
    }

    /// <summary>
    /// Resolves the entity prototype this disguise turns the borg into, if any.
    /// </summary>
    protected bool TryGetDisguisedPrototype(BorgDisguiseComponent comp, [NotNullWhen(true)] out EntityPrototype? disguisedProto)
    {
        return ProtoMan.Resolve(comp.DisguisedPrototype, out disguisedProto);
    }

    /// <summary>
    /// Swaps the shared parts of the entity's components based on the disguise state.
    /// </summary>
    protected void UpdateSharedAppearance(Entity<BorgDisguiseComponent> ent)
    {
        if (!TryPrototype(ent.Owner, out var entityPrototype))
            return;

        if (ent.Comp.Disguised && TryGetDisguisedPrototype(ent.Comp, out var disguisedPrototype))
        {
            _meta.SetEntityName(ent.Owner, disguisedPrototype.Name);
            _meta.SetEntityDescription(ent.Owner, disguisedPrototype.Description);
        }
        else
        {
            _meta.SetEntityName(ent.Owner, entityPrototype.Name);
            _meta.SetEntityDescription(ent.Owner, entityPrototype.Description);
        }
    }

    #region ActionManagement

    /// <summary>
    /// Gives the action to disguise
    /// </summary>
    private void OnMapInit(Entity<BorgDisguiseComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }

    /// <summary>
    /// Takes away the action to disguise from the entity.
    /// </summary>
    private void OnCompRemove(Entity<BorgDisguiseComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
    }

    #endregion
}

/// <summary>
/// Should be relayed upon using the action.
/// </summary>
public sealed partial class BorgDisguiseToggleActionEvent : InstantActionEvent;
