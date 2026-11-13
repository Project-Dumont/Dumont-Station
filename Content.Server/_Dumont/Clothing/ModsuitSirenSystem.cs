using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared._Dumont.Clothing;
using Content.Shared.Actions;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Light.Components;
using Content.Shared.PowerCell;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Dumont.Clothing;

/// <summary>
/// Grants the siren action to an equipped helmet while its parent suit has power.
/// A separate emitter preserves the helmet's existing flashlight and its battery.
/// </summary>
public sealed partial class ModsuitSirenSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedPowerCellSystem _powerCell = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly EntProtoId SirenAction = "ActionToggleModsuitParamedicSiren";
    private static readonly EntProtoId SirenEmitter = "ModsuitParamedicSirenEmitter";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ModsuitSirenComponent, ModsuitSirenToggleEvent>(OnToggle);
        SubscribeLocalEvent<ModsuitSirenComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Suit power and the attached helmet can change independently of sealing events.
        var query = EntityQueryEnumerator<ModsuitSirenComponent>();
        while (query.MoveNext(out var uid, out var component))
            Synchronize((uid, component));
    }

    /// <summary>
    /// Checks the equipped helmet and its parent suit, including actual cell charge.
    /// </summary>
    private EntityUid? GetWearer(EntityUid helmet)
    {
        if (!TryComp<AttachedClothingComponent>(helmet, out var attached)
            || !TryComp<SealableClothingControlComponent>(attached.AttachedUid, out var control)
            || control.WearerEntity is not {} wearer
            || !_inventory.TryGetSlotEntity(wearer, "head", out var head)
            || head != helmet
            || !_inventory.TryGetSlotEntity(wearer, "back", out var back)
            || back != attached.AttachedUid
            || !_powerCell.HasDrawCharge(attached.AttachedUid))
            return null;

        return wearer;
    }

    /// <summary>
    /// Removes the action and stops the siren when the helmet or suit becomes unavailable.
    /// Power restoration restores the button, but never automatically restarts the siren.
    /// </summary>
    private EntityUid? Synchronize(Entity<ModsuitSirenComponent> ent)
    {
        var wearer = GetWearer(ent);
        if (wearer == null)
        {
            _actions.RemoveAction(ent.Comp.Action);
            if (ent.Comp.Emitter is {} emitter && !TerminatingOrDeleted(emitter))
                _toggle.TryDeactivate(emitter, predicted: false);
            _actions.SetToggled(ent.Comp.Action, false);
            return null;
        }

        if (_actions.GetAction(ent.Comp.Action)?.Comp.AttachedEntity != wearer)
            _actions.AddAction(wearer.Value, ref ent.Comp.Action, SirenAction, ent);

        return wearer;
    }

    /// <summary>
    /// Toggles an independent sound and rotating-light emitter attached to the helmet.
    /// Revalidates suit state so a stale action cannot bypass power or equipment checks.
    /// </summary>
    private void OnToggle(Entity<ModsuitSirenComponent> ent, ref ModsuitSirenToggleEvent args)
    {
        if (args.Handled || Synchronize(ent) is not {} wearer || wearer != args.Performer)
            return;

        if (ent.Comp.Emitter == null || !Exists(ent.Comp.Emitter))
        {
            var emitter = Spawn(SirenEmitter, Transform(ent).Coordinates);
            _transform.SetParent(emitter, ent);
            _transform.SetLocalPosition(emitter, System.Numerics.Vector2.Zero);
            ent.Comp.Emitter = emitter;
        }

        args.Handled = _toggle.Toggle(ent.Comp.Emitter.Value, wearer, predicted: false);
        _actions.SetToggled(ent.Comp.Action, _toggle.IsActivated(ent.Comp.Emitter.Value));
        if (args.Handled && TryComp<HandheldLightComponent>(ent, out var light))
            _audio.PlayPvs(_toggle.IsActivated(ent.Comp.Emitter.Value) ? light.TurnOnSound : light.TurnOffSound, ent);
    }

    /// <summary>
    /// Cleans up the action and emitter when the helmet or its siren component is deleted.
    /// </summary>
    private void OnShutdown(Entity<ModsuitSirenComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.Action);
        if (ent.Comp.Emitter is {} emitter)
            QueueDel(emitter);
    }
}
