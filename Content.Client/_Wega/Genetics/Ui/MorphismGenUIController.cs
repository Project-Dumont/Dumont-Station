// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Genetics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.GameStates;

namespace Content.Client._Wega.Genetics.Ui;

public sealed class MorphismGenUIController : UIController
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    
    private MorphismGenPanel? _panel;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<MorphismMenuOpenedEvent>(OnMenuReceived);
    }

    private void OnMenuReceived(MorphismMenuOpenedEvent args, EntitySessionEventArgs eventArgs)
    {
        ShowPanel(args);
    }

    public void ShowPanel(MorphismMenuOpenedEvent state)
    {
        if (_panel == null)
        {
            _panel = _uiManager.CreateWindow<MorphismGenPanel>();
            _panel.OnClose += OnMenuClosed;
        }

        _panel.Initialize(state);
        _panel.OpenCentered();
    }

    private void OnMenuClosed()
    {
        _panel = null;
    }
}
