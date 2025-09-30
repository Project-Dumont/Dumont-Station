// SPDX-FileCopyrightText: 2025 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

//using Content.Shared._Gabystation.ATM;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;
using static Content.Shared.Pinpointer.SharedNavMapSystem;
using Robust.Client.UserInterface;
using Content.Shared._Gabystation.ATM;


namespace Content.Client._Gabystation.ATM;

[UsedImplicitly]
public sealed class BankATMBui : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entities = default!;

    [ViewVariables]
    private BankATMWindow? _window;
    private int _amount;

    public BankATMBui(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        if (_window != null)
            return;

        _window = this.CreateWindow<BankATMWindow>();
        _window.OpenCentered();
        _window.WithdrawTabButton.OnPressed += _ => View(ViewType.Withdraw);
        _window.DepositTabButton.OnPressed += _ => View(ViewType.Deposit);

        _window.WithdrawInput.OnTextChanged += _ =>
        {
            _window.WithdrawButton.Disabled = !int.TryParse(_window.WithdrawInput.Text, out var amount) || amount <= 0 || amount > 999;
            _amount = amount;
        };
        _window.WithdrawButton.OnPressed += _ =>
        {
            SendMessage(new BankATMMessage()
            {
                Type = BankATMMsgType.Withdraw,
                Amount = _amount
            });
            //Close();
        };
        _window.DepositButton.OnPressed += _ =>
        {
            SendMessage(new BankATMMessage()
            {
                Type = BankATMMsgType.Deposit
            });
            //Close();
        };

        View(ViewType.Withdraw);
    }
    protected override void UpdateState(BoundUserInterfaceState? state)
    {
        if (state is not BankATMBuiState s)
            return;

        //View(ViewType.Withdraw);
        RefreshUI(s);
    }

    private void RefreshUI(BankATMBuiState state)
    {
        if (_window == null)
            return;

        _window.UpdateState(state);

        //_window.BalanceLabel.Children.Clear();
    }

    private void View(ViewType type)
    {
        if (_window == null)
            return;

        _window.WithdrawTabButton.Parent!.Margin = new Thickness(0, 0, 0, 10);
        _window.WithdrawTab.Visible = type == ViewType.Withdraw;
        _window.DepositTab.Visible = type == ViewType.Deposit;
    }

    private enum ViewType
    {
        Withdraw,
        Deposit
    }
}
