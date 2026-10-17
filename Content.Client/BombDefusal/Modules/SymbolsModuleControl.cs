using System.Numerics;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

/// <summary>
/// UI control for the "Symbols" module.
/// Displays 4 symbol buttons in a 2x2 grid; player presses in correct order.
/// </summary>
public sealed class SymbolsModuleControl : BaseModuleControl
{
    private readonly GridContainer _grid;

    /// <summary>
    /// I can't figure out how to display symbols in Unicode that aren't supported in the game, so 2 letter combos! YAY!
    /// </summary>
    private static readonly string[] SymbolGlyphs =
    {
        "AX", "BQ", "CZ", "DW", "EK",   // 0-4
        "FJ", "GN", "HV", "IP", "LR",   // 5-9
        "MT", "NU", "OY", "PH", "QS",   // 10-14
        "RD", "SF", "TG", "UB", "VA",   // 15-19
        "WE", "XL", "YM", "ZC", "KO",   // 20-24
    };

    private readonly List<Button> _symbolButtons = new();
    private readonly List<PanelContainer> _symbolPanels = new();

    public SymbolsModuleControl()
    {
        SetHeaderText(Loc.GetString("bomb-defusal-module-symbols"));

        _grid = new GridContainer
        {
            Columns = 2,
            HorizontalExpand = true,
        };
        ContentContainer.AddChild(_grid);
    }

    public override void UpdateModuleState(BombDefusalModuleState state)
    {
        if (state is not SymbolsModuleState symbolsState)
            return;

        if (_symbolButtons.Count == 0)
        {
            _grid.RemoveAllChildren();
            _symbolPanels.Clear();

            for (var i = 0; i < symbolsState.SymbolIds.Count; i++)
            {
                var symbolIndex = i;
                var symbolId = symbolsState.SymbolIds[i];

                var glyph = symbolId >= 0 && symbolId < SymbolGlyphs.Length
                    ? SymbolGlyphs[symbolId]
                    : "?";

                // Wrap button in a panel
                var buttonPanel = new PanelContainer
                {
                    Margin = new Thickness(2),
                    HorizontalExpand = true,
                };
                buttonPanel.PanelOverride = new StyleBoxFlat
                {
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 2,
                    ContentMarginRightOverride = 2,
                    ContentMarginTopOverride = 2,
                    ContentMarginBottomOverride = 2,
                };

                var button = new Button
                {
                    Text = glyph,
                    MinSize = new Vector2(56, 56),
                    HorizontalExpand = true,
                };

                var idx = symbolIndex;
                button.OnPressed += _ => RaiseAction(new PressSymbolAction(idx));

                buttonPanel.AddChild(button);
                _grid.AddChild(buttonPanel);

                _symbolButtons.Add(button);
                _symbolPanels.Add(buttonPanel);
            }
        }

        for (var i = 0; i < symbolsState.SymbolIds.Count; i++)
        {
            var isPressed = symbolsState.PressedSymbols.Contains(i);
            var button = _symbolButtons[i];
            var panel = _symbolPanels[i];
            var styleBox = (StyleBoxFlat) panel.PanelOverride!;

            styleBox.BackgroundColor = isPressed ? Color.FromHex("#1a3d1a") : Color.FromHex("#111122");
            styleBox.BorderColor = isPressed ? Color.FromHex("#00ff41") : Color.FromHex("#333355");

            button.Disabled = isPressed || symbolsState.IsSolved;

            if (isPressed)
                button.ModulateSelfOverride = Color.FromHex("#336633");
            else
                button.ModulateSelfOverride = null;
        }
    }
}
