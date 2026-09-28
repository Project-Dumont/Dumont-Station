// SPDX-License-Identifier: AGPL-3.0-or-later

// Dumont start
using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

using Content.Trauma.Shared.Heretic.Messages;
using JetBrains.Annotations;
// Dumont end

// Dumont start
using Robust.Client.UserInterface;
// Dumont end

namespace Content.Trauma.Client.Heretic.UI;

[UsedImplicitly]
public sealed partial class FeastOfOwlsBoundUserInterface(EntityUid owner, Enum uiKey)
    : BoundUserInterface(owner, uiKey)
{
    private FeastOfOwlsMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<FeastOfOwlsMenu>();
        _menu.AcceptButton.OnPressed += _ =>
        {
            SendMessage(new FeastOfOwlsMessage(true));
            Close();
        };
        _menu.DenyButton.OnPressed += _ =>
        {
            SendMessage(new FeastOfOwlsMessage(false));
            Close();
        };

        _menu.OpenCentered();
    }
}
