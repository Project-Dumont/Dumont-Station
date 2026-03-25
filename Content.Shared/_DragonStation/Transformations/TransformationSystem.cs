using Content.Shared.Actions;
using Content.Shared.Damage.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Inventory;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Toggleable;
using Content.Shared._DragonStation.FighterProgression;
using Content.Shared._DragonStation.PowerLevel;
using Robust.Shared.Network;
using Robust.Shared.Timing;


namespace Content.Shared._DragonStation.Transformations;

public sealed partial class TransformationSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedFighterProgressionSystem _fighterProgression = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TransformationComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TransformationComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TransformationComponent, ToggleActionEvent>(OnToggleAction);
        InitializeStamina();
        InitializeCombat();
        InitializeInventory();
        InitializeRanged();
    }

    private void OnStartup(EntityUid uid, TransformationComponent component, ComponentStartup args)
    {
        RefreshActionAvailability(uid, component);
    }

    private void OnShutdown(EntityUid uid, TransformationComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.ToggleActionEntity);
        DisableAura(uid);
    }

    private void OnToggleAction(EntityUid uid, TransformationComponent component, ToggleActionEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!CanUseTransformation(uid, component))
            return;

        if (component.Active)
        {
            DisableTransformation(uid, component);
            args.Handled = true;
            return;
        }

        component.Active = true;

        if (_net.IsServer)
        {
            if (component.StripBlockedGearOnTransform)
                StripBlockedGear(uid, component);

            if (component.DropHeldGunsOnTransform)
                DropHeldGuns(uid, component);

            if (component.GrantsReflection)
                EnableReflection(uid, component);
        }

        if (_net.IsServer)
            EnableAura(uid, component);
        _actions.SetToggled(component.ToggleActionEntity, true);
        _movement.RefreshMovementSpeedModifiers(uid);
        RaiseLocalEvent(uid, new PowerLevelRefreshRequestedEvent(), true);

        if (_net.IsServer)
            _popup.PopupEntity(Loc.GetString(component.ActivationPopup), uid, uid);

        Dirty(uid, component);
        args.Handled = true;
    }

    private bool CanUseTransformation(EntityUid uid, TransformationComponent component)
    {
        return component.RequiredFighterSkill == null || _fighterProgression.HasSkill(uid, component.RequiredFighterSkill);
    }

    private void RefreshActionAvailability(EntityUid uid, TransformationComponent component)
    {
        if (CanUseTransformation(uid, component))
            _actions.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
        else
            _actions.RemoveAction(uid, component.ToggleActionEntity);
    }

    private void DisableTransformation(EntityUid uid, TransformationComponent component)
    {
        if (!component.Active)
            return;

        component.Active = false;
        _actions.SetToggled(component.ToggleActionEntity, false);

        if (_net.IsServer)
            _popup.PopupEntity(Loc.GetString(component.DeactivationPopup), uid, uid);

        _movement.RefreshMovementSpeedModifiers(uid);
        RaiseLocalEvent(uid, new PowerLevelRefreshRequestedEvent(), true);
        if (_net.IsServer && component.GrantsReflection)
            DisableReflection(uid);

        if (_net.IsServer)
            DisableAura(uid);

        Dirty(uid, component);
    }
}
