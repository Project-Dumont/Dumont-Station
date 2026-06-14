using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

/// <summary>
/// UI control for the "Codewords" module.
/// Displays 6 words as buttons; the player selects the correct one.
/// </summary>
public sealed class CodewordsModuleControl : BaseModuleControl
{
    private readonly Label _title;
    private readonly BoxContainer _wordsContainer;
    private readonly Label _solvedLabel;

    public CodewordsModuleControl()
    {
        _title = new Label
        {
            Text = Loc.GetString("bomb-defusal-module-codewords"),
            FontColorOverride = Color.FromHex("#00ff41"),
            Margin = new Thickness(0, 0, 0, 4),
        };
        AddChild(_title);

        _wordsContainer = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };
        AddChild(_wordsContainer);

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
        if (state is not CodewordsModuleState codewordsState)
            return;

        _solvedLabel.Visible = codewordsState.IsSolved;

        _wordsContainer.RemoveAllChildren();

        for (var i = 0; i < codewordsState.Words.Count; i++)
        {
            var wordIndex = i;
            var word = codewordsState.Words[i];

            var button = new Button
            {
                Text = word,
                Disabled = codewordsState.IsSolved,
                Margin = new Thickness(0, 1),
                HorizontalExpand = true,
            };

            var idx = wordIndex;
            button.OnPressed += _ => RaiseAction(new SubmitCodewordAction(idx));

            _wordsContainer.AddChild(button);
        }
    }
}
