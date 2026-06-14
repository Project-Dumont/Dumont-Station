using System.Numerics;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.BombDefusal.Modules;

/// <summary>
/// UI control for "The Button" module.
/// A large colored button with a label. Supports press and hold/release.
/// </summary>
public sealed class ButtonModuleControl : BaseModuleControl
{
    private readonly Label _title;
    private readonly Button _button;
    private readonly Label _stripLabel;
    private readonly Label _solvedLabel;

    private static readonly Dictionary<ButtonColor, Color> ButtonColorMap = new()
    {
        { ButtonColor.Red, Color.FromHex("#cc3333") },
        { ButtonColor.Blue, Color.FromHex("#3333cc") },
        { ButtonColor.Yellow, Color.FromHex("#cccc33") },
        { ButtonColor.White, Color.FromHex("#cccccc") },
    };

    private static readonly Dictionary<ButtonColor, string> StripColorNames = new()
    {
        { ButtonColor.Red, "RED" },
        { ButtonColor.Blue, "BLUE" },
        { ButtonColor.Yellow, "YELLOW" },
        { ButtonColor.White, "WHITE" },
    };

    // For tracking how long button has been held (to distinguish tap vs hold on client side)
    private bool _isHolding;

    public ButtonModuleControl()
    {
        _title = new Label
        {
            Text = Loc.GetString("bomb-defusal-module-button"),
            FontColorOverride = Color.FromHex("#00ff41"),
            Margin = new Thickness(0, 0, 0, 4),
        };
        AddChild(_title);

        _button = new Button
        {
            Text = "PRESS",
            MinSize = new Vector2(120, 60),
            HorizontalExpand = true,
            Margin = new Thickness(4),
        };
        _button.OnKeyBindDown += _ =>
        {
            if (!_isHolding)
            {
                _isHolding = true;
                RaiseAction(new PressButtonAction());
            }
        };
        _button.OnKeyBindUp += _ =>
        {
            if (_isHolding)
            {
                _isHolding = false;
                // Send the current timer digit (last digit of seconds)
                // The server will validate
                RaiseAction(new ReleaseButtonAction(_currentTimerDigit));
            }
        };
        AddChild(_button);

        _stripLabel = new Label
        {
            Text = "",
            Visible = false,
            Align = Label.AlignMode.Center,
            Margin = new Thickness(0, 4, 0, 0),
        };
        AddChild(_stripLabel);

        _solvedLabel = new Label
        {
            Text = Loc.GetString("bomb-defusal-module-solved"),
            FontColorOverride = Color.FromHex("#00ff41"),
            Visible = false,
            Align = Label.AlignMode.Center,
        };
        AddChild(_solvedLabel);
    }

    private int _currentTimerDigit;

    /// <summary>
    /// Called by the parent menu to pass the current timer value for release-digit validation.
    /// </summary>
    public void SetTimerDigit(int digit)
    {
        _currentTimerDigit = digit;
    }

    public override void UpdateState(BombDefusalModuleState state)
    {
        if (state is not ButtonModuleState buttonState)
            return;

        _solvedLabel.Visible = buttonState.IsSolved;
        _button.Disabled = buttonState.IsSolved;

        var labelText = buttonState.Label switch
        {
            ButtonLabel.Abort => "ABORT",
            ButtonLabel.Detonate => "DETONATE",
            ButtonLabel.Hold => "HOLD",
            ButtonLabel.Press => "PRESS",
            _ => "???",
        };
        _button.Text = labelText;

        // Color the button
        if (ButtonColorMap.TryGetValue(buttonState.Color, out var color))
            _button.ModulateSelfOverride = color;

        // Show strip color when held
        if (buttonState.IsHeld && buttonState.StripColor != null)
        {
            _stripLabel.Visible = true;
            var stripName = StripColorNames.GetValueOrDefault(buttonState.StripColor.Value, "???");
            _stripLabel.Text = $"Strip: {stripName}";
            if (ButtonColorMap.TryGetValue(buttonState.StripColor.Value, out var stripCol))
                _stripLabel.FontColorOverride = stripCol;
        }
        else
        {
            _stripLabel.Visible = false;
        }
    }
}
