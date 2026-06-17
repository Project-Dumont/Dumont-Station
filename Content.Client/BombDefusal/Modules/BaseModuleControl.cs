using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

/// <summary>
/// Base class for module UI controls in the bomb defusal interface.
/// Provides a consistent KTANE-style frame with dark background, colored indicator strip,
/// module header, and solved/unsolved state.
/// </summary>
public abstract class BaseModuleControl : BoxContainer
{
    public int ModuleIndex;
    public event Action<BombModuleAction>? OnAction;

    private readonly PanelContainer _backgroundPanel;
    private readonly PanelContainer _indicatorStrip;
    private readonly Label _headerLabel;
    private readonly Label _solvedIndicator;
    protected readonly BoxContainer ContentContainer;

    private bool _isSolved;

    protected void RaiseAction(BombModuleAction action)
    {
        OnAction?.Invoke(action);
    }

    public abstract void UpdateModuleState(BombDefusalModuleState state);

    public void UpdateState(BombDefusalModuleState state)
    {
        _isSolved = state.IsSolved;
        UpdateIndicator();
        UpdateModuleState(state);
    }

    protected BaseModuleControl()
    {
        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;
        VerticalExpand = true;
        Margin = new Thickness(3);

        // Outer background panel — dark card style
        _backgroundPanel = new PanelContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        var backgroundBox = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#0d0d1a"),
            BorderColor = Color.FromHex("#333355"),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 0,
            ContentMarginRightOverride = 0,
            ContentMarginTopOverride = 0,
            ContentMarginBottomOverride = 0,
        };
        _backgroundPanel.PanelOverride = backgroundBox;

        var innerLayout = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        // Left-side colored indicator strip
        _indicatorStrip = new PanelContainer
        {
            MinWidth = 4,
            MaxWidth = 4,
            VerticalExpand = true,
        };
        _indicatorStrip.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#ff4444"),
        };
        innerLayout.AddChild(_indicatorStrip);

        // Main content area
        var mainContent = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Margin = new Thickness(6, 4),
        };

        // Header row with module name and solved indicator
        var headerRow = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Margin = new Thickness(0, 0, 0, 4),
        };

        _headerLabel = new Label
        {
            Text = "MODULE",
            FontColorOverride = Color.FromHex("#8888aa"),
            HorizontalExpand = true,
        };
        headerRow.AddChild(_headerLabel);

        _solvedIndicator = new Label
        {
            Text = "✕",
            FontColorOverride = Color.FromHex("#ff4444"),
        };
        headerRow.AddChild(_solvedIndicator);

        mainContent.AddChild(headerRow);

        // Separator line
        var separator = new PanelContainer
        {
            MinHeight = 1,
            MaxHeight = 1,
            HorizontalExpand = true,
            Margin = new Thickness(0, 0, 0, 4),
        };
        separator.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#333355"),
        };
        mainContent.AddChild(separator);

        // Content container for subclass controls
        ContentContainer = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        mainContent.AddChild(ContentContainer);

        innerLayout.AddChild(mainContent);
        _backgroundPanel.AddChild(innerLayout);
        AddChild(_backgroundPanel);
    }

    protected void SetHeaderText(string text)
    {
        _headerLabel.Text = text.ToUpperInvariant();
    }

    private void UpdateIndicator()
    {
        if (_isSolved)
        {
            _solvedIndicator.Text = "✓";
            _solvedIndicator.FontColorOverride = Color.FromHex("#00ff41");
            ((StyleBoxFlat) _indicatorStrip.PanelOverride!).BackgroundColor = Color.FromHex("#00ff41");
        }
        else
        {
            _solvedIndicator.Text = "✕";
            _solvedIndicator.FontColorOverride = Color.FromHex("#ff4444");
            ((StyleBoxFlat) _indicatorStrip.PanelOverride!).BackgroundColor = Color.FromHex("#ff4444");
        }
    }
}
