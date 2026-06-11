// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Anomaly;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Anomaly.Ui;

[UsedImplicitly]
public sealed class AdvancedAnomalyGeneratorBoundUserInterface : BoundUserInterface
{
    private AdvancedAnomalyGeneratorWindow? _window;

    public AdvancedAnomalyGeneratorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<AdvancedAnomalyGeneratorWindow>();
        _window.OnGenerate += (entry, x, y) => SendMessage(new AdvancedAnomalyGeneratorGenerateMessage(entry, x, y));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is AdvancedAnomalyGeneratorUserInterfaceState msg)
            _window?.UpdateState(msg);
    }
}
