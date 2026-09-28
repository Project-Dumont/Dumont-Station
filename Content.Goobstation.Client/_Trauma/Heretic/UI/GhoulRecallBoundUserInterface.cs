// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Ui;
using JetBrains.Annotations;
// Dumont end

// Dumont start
using Robust.Client.UserInterface;
// Dumont end

namespace Content.Trauma.Client.Heretic.UI;

[UsedImplicitly]
public sealed partial class GhoulRecallBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private GhoulRecallWindow _window = new();

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<GhoulRecallWindow>();
        _window.OnClose += Close;
        _window.OnItemSelected += SendMessage;
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not HereticGhoulRecallUiState recallState)
            return;

        _window.Populate(recallState.Ghouls);
    }

    private void SendMessage(NetEntity ghoul)
    {
        SendPredictedMessage(new HereticGhoulRecallMessage(ghoul));
    }
}
