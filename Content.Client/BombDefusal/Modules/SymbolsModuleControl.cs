using System.Numerics;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

/// <summary>
/// UI control for the "Symbols" module.
/// Displays 4 symbol buttons in a 2x2 grid; player presses in correct order.
/// </summary>
public sealed class SymbolsModuleControl : BaseModuleControl
{
    private readonly Label _title;
    private readonly GridContainer _grid;
    private readonly Label _solvedLabel;

    /// <summary>
    /// Unicode symbol glyphs used for display. Index matches symbol ID.
    /// These are decorative characters that look like mysterious runes/symbols.
    /// </summary>
    private static readonly string[] SymbolGlyphs =
    {
        "Ω", "Ψ", "Ξ", "Φ", "Σ",   // 0-4
        "Δ", "Π", "Θ", "Λ", "Γ",   // 5-9
        "ℌ", "℘", "ℜ", "ℑ", "ℵ",   // 10-14
        "♠", "♣", "♦", "♥", "★",   // 15-19
        "☆", "◆", "◇", "▲", "▼",   // 20-24
    };

    public SymbolsModuleControl()
    {
        _title = new Label
        {
            Text = Loc.GetString("bomb-defusal-module-symbols"),
            FontColorOverride = Color.FromHex("#00ff41"),
            Margin = new Thickness(0, 0, 0, 4),
        };
        AddChild(_title);

        _grid = new GridContainer
        {
            Columns = 2,
            HorizontalExpand = true,
        };
        AddChild(_grid);

        _solvedLabel = new Label
        {
            Text = Loc.GetString("bomb-defusal-module-solved"),
            FontColorOverride = Color.FromHex("#00ff41"),
            Visible = false,
            Align = Label.AlignMode.Center,
        };
        AddChild(_solvedLabel);
    }

    public override void UpdateState(BombDefusalModuleState state)
    {
        if (state is not SymbolsModuleState symbolsState)
            return;

        _solvedLabel.Visible = symbolsState.IsSolved;

        _grid.RemoveAllChildren();

        for (var i = 0; i < symbolsState.SymbolIds.Count; i++)
        {
            var symbolIndex = i;
            var symbolId = symbolsState.SymbolIds[i];
            var isPressed = symbolsState.PressedSymbols.Contains(i);

            var glyph = symbolId >= 0 && symbolId < SymbolGlyphs.Length
                ? SymbolGlyphs[symbolId]
                : "?";

            var button = new Button
            {
                Text = glyph,
                MinSize = new Vector2(60, 60),
                Disabled = isPressed || symbolsState.IsSolved,
                Margin = new Thickness(2),
                HorizontalExpand = true,
            };

            if (isPressed)
                button.ModulateSelfOverride = Color.FromHex("#336633");

            var idx = symbolIndex;
            button.OnPressed += _ => RaiseAction(new PressSymbolAction(idx));

            _grid.AddChild(button);
        }
    }
}
