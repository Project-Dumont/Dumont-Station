using Content.Shared._CMU14.Xenomorphs;
using Content.Client._Dumont.Xenomorphs;
using Content.Shared._Dumont.Xenomorphs;
using Content.Shared.ActionBlocker;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;

namespace Content.Client._CMU14.Xenomorphs;

public sealed partial class XenomorphBarrageInputSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private IInputManager _input = default!;
    [Dependency] private MapSystem _map = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private TransformSystem _transform = default!;

    private EntityQuery<XenomorphDespoilerComponent> _armedQuery;
    private EntityUid? _pressedEntity;

    public override void Initialize()
    {
        _armedQuery = GetEntityQuery<XenomorphDespoilerComponent>();

        CommandBinds.Builder
            .Bind(EngineKeyFunctions.Use, new PointerInputCmdHandler(OnUse, ignoreUp: false, outsidePrediction: true))
            .Register<XenomorphBarrageInputSystem>();
    }

    public override void Shutdown()
    {
        CommandBinds.Unregister<XenomorphBarrageInputSystem>();
    }

    private bool OnUse(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        if (_player.LocalEntity is not { } ent)
            return false;

        if (args.State == BoundKeyState.Down)
        {
            if (!_armedQuery.TryComp(ent, out var state) || !state.Armed || state.Charging)
                return false;

            if (!_actionBlocker.CanConsciouslyPerformAction(ent))
                return false;

            if (!TryGetCursorCoords(out var coords))
                return false;

            _pressedEntity = ent;
            RaiseNetworkEvent(new XenomorphBarrageInputEvent(GetNetCoordinates(coords), false));
            return true;
        }

        if (args.State == BoundKeyState.Up)
        {
            var pressed = _pressedEntity == ent;
            _pressedEntity = null;
            if (!pressed && (!_armedQuery.TryComp(ent, out var state) || !state.Charging))
                return false;

            if (!TryGetCursorCoords(out var coords))
                return false;

            RaiseNetworkEvent(new XenomorphBarrageInputEvent(GetNetCoordinates(coords), true));
            return true;
        }

        return false;
    }

    private bool TryGetCursorCoords(out EntityCoordinates coords)
    {
        coords = default;
        var mousePos = _eye.PixelToMap(_input.MouseScreenPosition);

        EntityUid grid;
        if (_map.TryFindGridAt(mousePos, out var gridUid, out _))
            grid = gridUid;
        else if (_map.TryGetMap(mousePos.MapId, out var map))
            grid = map.Value;
        else
            return false;

        coords = _transform.ToCoordinates(grid, mousePos);
        return true;
    }
}
