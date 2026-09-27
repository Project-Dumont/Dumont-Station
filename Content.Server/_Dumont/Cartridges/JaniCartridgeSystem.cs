using Content.Server.Pinpointer;
using Content.Shared.CartridgeLoader;
using Content.Shared.PDA;
using Content.Shared.Popups;
using Content.Server._Dumont.CartridgeLoader.Cartridges;
using Robust.Shared.Timing;
using Robust.Shared.Maths;

public sealed partial class JaniCartridgeSystem : EntitySystem {
    [Dependency] private NavMapSystem _navMapSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _xform = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<JaniCartridgeComponent, CartridgeActivatedEvent>(OnActivate);
    }

    private void OnActivate(Entity<JaniCartridgeComponent> cart, ref CartridgeActivatedEvent args)
    {
        if (!_navMapSystem.TryGetNearestBeacon(_xform.GetMapCoordinates(cart.Owner), out var beacon, out var _) || beacon is null)
            return;

        if (_timing.CurTime - cart.Comp.LastUsed < cart.Comp.CallDelay) {
            _popup.PopupEntity(Loc.GetString("jani-cartridge-wait"), cart.Owner);
            return;
        }

        cart.Comp.LastUsed = _timing.CurTime;

        var location = beacon.Value.Comp.Text ?? Loc.GetString("generic-unknown");
        var color = beacon.Value.Comp.Color;

        var message = Loc.GetString(cart.Comp.LocalizedMessage,
            ("location", location),
            ("color", color.ToHex() ?? "#FFFFFF"));

        PdaNotificationEvent ev = new(message, cart.Comp.NotificationGroup, true);
        RaiseLocalEvent(ev);
        _popup.PopupEntity(Loc.GetString("jani-cartridge-message-sent"), cart.Owner);
    }
}
