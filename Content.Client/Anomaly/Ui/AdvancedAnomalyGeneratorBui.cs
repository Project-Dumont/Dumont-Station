// SPDX-FileCopyrightText: 2026 Dumont Station Contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Anomaly;
using Content.Shared.Research.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Anomaly.Ui;

[UsedImplicitly]
public sealed class AdvancedAnomalyGeneratorBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private AdvancedAnomalyGeneratorWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<AdvancedAnomalyGeneratorWindow>();
        _window.OnGenerate += (entry, tile) => SendMessage(new AdvancedAnomalyGeneratorGenerateMessage(entry, tile));
        // Opens the usual ResearchClient window for picking a research server
        _window.OnServerSelection += () => SendMessage(new ConsoleServerSelectionMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is AdvancedAnomalyGeneratorUserInterfaceState cast)
            _window?.UpdateState(cast);
    }
}
