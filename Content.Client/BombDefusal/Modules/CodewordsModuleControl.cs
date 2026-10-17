using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

/// <summary>
/// UI control for the "Codewords" module.
/// Displays 6 words as styled buttons; the player selects the correct one.
/// </summary>
public sealed class CodewordsModuleControl : BaseModuleControl
{
    private readonly BoxContainer _wordsContainer;

    private readonly List<Button> _wordButtons = new();

    public CodewordsModuleControl()
    {
        SetHeaderText(Loc.GetString("bomb-defusal-module-codewords"));

        _wordsContainer = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };
        ContentContainer.AddChild(_wordsContainer);
    }

    public override void UpdateModuleState(BombDefusalModuleState state)
    {
        if (state is not CodewordsModuleState codewordsState)
            return;

        if (_wordButtons.Count == 0)
        {
            _wordsContainer.RemoveAllChildren();
            for (var i = 0; i < codewordsState.Words.Count; i++)
            {
                var wordIndex = i;
                var word = codewordsState.Words[i];

                // Wrap in a panel
                var wordPanel = new PanelContainer
                {
                    HorizontalExpand = true,
                    Margin = new Thickness(0, 1),
                };
                wordPanel.PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = Color.FromHex("#0a0a15"),
                    BorderColor = Color.FromHex("#222244"),
                    BorderThickness = new Thickness(1),
                    ContentMarginLeftOverride = 2,
                    ContentMarginRightOverride = 2,
                    ContentMarginTopOverride = 1,
                    ContentMarginBottomOverride = 1,
                };

                var button = new Button
                {
                    Text = word,
                    HorizontalExpand = true,
                };

                var idx = wordIndex;
                button.OnPressed += _ => RaiseAction(new SubmitCodewordAction(idx));

                wordPanel.AddChild(button);
                _wordsContainer.AddChild(wordPanel);
                _wordButtons.Add(button);
            }
        }

        for (var i = 0; i < codewordsState.Words.Count; i++)
        {
            var wordIndex = i;
            var button = _wordButtons[i];

            button.Disabled = codewordsState.IsSolved;

            if (codewordsState.IsSolved && codewordsState.SelectedIndex == wordIndex)
            {
                button.ModulateSelfOverride = Color.FromHex("#336633");
            }
            else
            {
                button.ModulateSelfOverride = null;
            }
        }
    }
}
