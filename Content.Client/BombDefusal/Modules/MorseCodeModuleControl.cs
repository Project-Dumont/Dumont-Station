using System;
using System.Collections.Generic;
using System.Numerics;
using Content.Shared.BombDefusal;
using Content.Shared.BombDefusal.Modules;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client.BombDefusal.Modules;

public sealed class MorseCodeModuleControl : BaseModuleControl
{
    private readonly PanelContainer _lightIndicator;
    private readonly Label _freqLabel;
    private readonly Button _btnUp;
    private readonly Button _btnDown;
    private readonly Button _btnTx;

    private readonly StyleBoxFlat _lightOn;
    private readonly StyleBoxFlat _lightOff;

    private string _currentMorse = string.Empty;
    private readonly List<bool> _flashTimeline = new();
    private int _timelineIndex;
    private float _timer;
    private const float UnitTime = 0.20f; // Seconds per unit

    private bool _isSolved;

    public MorseCodeModuleControl()
    {
        SetHeaderText(Loc.GetString("bomb-defusal-module-morsecode"));

        _lightOn = new StyleBoxFlat { BackgroundColor = Color.FromHex("#ffdd33") };
        _lightOff = new StyleBoxFlat { BackgroundColor = Color.FromHex("#111100"), BorderColor = Color.FromHex("#333300"), BorderThickness = new Thickness(1) };

        var mainLayout = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Align = BoxContainer.AlignMode.Center,
        };
        ContentContainer.AddChild(mainLayout);

        // Flashing light area
        var lightPanel = new PanelContainer
        {
            MinSize = new Vector2(40, 40),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12),
        };
        _lightIndicator = new PanelContainer
        {
            MinSize = new Vector2(30, 30),
            PanelOverride = _lightOff,
        };
        lightPanel.AddChild(_lightIndicator);
        mainLayout.AddChild(lightPanel);

        // Frequency display and tuning area
        var tuneRow = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16),
        };
        mainLayout.AddChild(tuneRow);

        _btnDown = new Button { Text = "◀", MinSize = new Vector2(30, 30) };
        _btnDown.OnPressed += _ => RaiseAction(new CycleMorseFrequencyAction(false));
        tuneRow.AddChild(_btnDown);

        var freqPanel = new PanelContainer
        {
            MinSize = new Vector2(100, 30),
            Margin = new Thickness(6, 0),
        };
        freqPanel.PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#000000"),
            BorderColor = Color.FromHex("#333355"),
            BorderThickness = new Thickness(1),
        };

        _freqLabel = new Label
        {
            Text = "3.500 MHz",
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            FontColorOverride = Color.FromHex("#00ff41"),
        };
        freqPanel.AddChild(_freqLabel);
        tuneRow.AddChild(freqPanel);

        _btnUp = new Button { Text = "▶", MinSize = new Vector2(30, 30) };
        _btnUp.OnPressed += _ => RaiseAction(new CycleMorseFrequencyAction(true));
        tuneRow.AddChild(_btnUp);

        // TX Button
        _btnTx = new Button
        {
            Text = "TX (TRANSMIT)",
            MinSize = new Vector2(120, 32),
            HorizontalAlignment = HAlignment.Center,
        };
        _btnTx.OnPressed += _ => RaiseAction(new SubmitMorseAction());
        mainLayout.AddChild(_btnTx);
    }

    public override void UpdateModuleState(BombDefusalModuleState state)
    {
        if (state is not MorseCodeModuleState morseState)
            return;

        _isSolved = morseState.IsSolved;
        _freqLabel.Text = $"{morseState.CurrentFrequency:F3} MHz";

        _btnUp.Disabled = _isSolved;
        _btnDown.Disabled = _isSolved;
        _btnTx.Disabled = _isSolved;

        if (_currentMorse != morseState.MorseSequence)
        {
            _currentMorse = morseState.MorseSequence;
            CompileTimeline(_currentMorse);
        }
    }

    private void CompileTimeline(string morse)
    {
        _flashTimeline.Clear();
        _timelineIndex = 0;
        _timer = 0;

        if (string.IsNullOrWhiteSpace(morse))
            return;

        var letters = morse.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int l = 0; l < letters.Length; l++)
        {
            var letter = letters[l];
            for (int c = 0; c < letter.Length; c++)
            {
                var ch = letter[c];
                if (ch == '.')
                {
                    // Dot: 1 unit ON, 1 unit OFF
                    _flashTimeline.Add(true);
                    _flashTimeline.Add(false);
                }
                else if (ch == '-')
                {
                    // Dash: 3 units ON, 1 unit OFF
                    _flashTimeline.Add(true);
                    _flashTimeline.Add(true);
                    _flashTimeline.Add(true);
                    _flashTimeline.Add(false);
                }
            }

            // Letter spacing: 3 units OFF (we already had 1 unit OFF at the end of the last character, so we add 2 more)
            if (l < letters.Length - 1)
            {
                _flashTimeline.Add(false);
                _flashTimeline.Add(false);
            }
        }

        // Word spacing: 7 units OFF (we had 1 unit at character end, so add 6 more units)
        for (int i = 0; i < 6; i++)
        {
            _flashTimeline.Add(false);
        }
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_isSolved || _flashTimeline.Count == 0)
        {
            _lightIndicator.PanelOverride = _lightOff;
            return;
        }

        _timer += args.DeltaSeconds;
        if (_timer >= UnitTime)
        {
            _timer -= UnitTime;
            _timelineIndex = (_timelineIndex + 1) % _flashTimeline.Count;
            _lightIndicator.PanelOverride = _flashTimeline[_timelineIndex] ? _lightOn : _lightOff;
        }
    }
}
