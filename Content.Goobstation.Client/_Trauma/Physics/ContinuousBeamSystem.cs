// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;
// Dumont end

using Content.Trauma.Shared.Heretic.Events;
using Content.Trauma.Shared.Physics.ComplexJoint;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Map;

// Dumont start
using Robust.Client.Graphics;
// Dumont end

// Dumont start
using Robust.Client.GameObjects;
// Dumont end

namespace Content.Trauma.Client.Physics;

public sealed partial class ContinuousBeamSystem : SharedContinuousBeamSystem
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IEyeManager _eye = default!;
    [Dependency] private IInputManager _input = default!;
    [Dependency] private InputSystem _inputSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesOutsidePrediction = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!Timing.IsFirstTimePredicted)
            return;

        if (_player.LocalEntity is not { } player)
            return;

        if (!TryGetGun(player, out var gun))
            return;

        var mousePos = _eye.PixelToMap(_input.MouseScreenPosition);

        if (mousePos.MapId == MapId.Nullspace)
            return;

        var keyFunc = gun.Value.Comp.AltFire ? EngineKeyFunctions.UseSecondary : EngineKeyFunctions.Use;
        var requestFire = CanFire(player, gun.Value) && _inputSystem.CmdStates.GetState(keyFunc) == BoundKeyState.Down;

        var coordinates = Xform.ToCoordinates(gun.Value.Owner, mousePos);

        RaisePredictiveEvent(new LaserBeamEndpointPositionEvent(GetNetCoordinates(coordinates), requestFire));
    }
}
