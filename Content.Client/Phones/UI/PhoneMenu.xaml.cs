// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Linq;
using Content.Client.UserInterface.Controls;
using Content.Shared.Phones;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.Phones.UI;

public sealed partial class PhoneMenu : FancyWindow
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public event Action<int>? OnKeypadButtonPressed;
    public event Action<int>? OnPhoneBookButtonPressed;
    public event Action? OnClearButtonPressed;
    public event Action? OnEnterButtonPressed;

    private readonly Dictionary<string, Control> _categoryContainers = new();

    private GridContainer _keypadGrid = default!;
    private BoxContainer _phoneBookContainer = default!;
    private RichTextLabel _dialNumber = default!;

    public PhoneMenu()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);

        _keypadGrid = this.FindControl<GridContainer>("KeypadGrid");
        _phoneBookContainer = this.FindControl<BoxContainer>("PhoneBookContainer");
        _dialNumber = this.FindControl<RichTextLabel>("DialNumber");

        FillKeypadGrid();
        FillCategories();
    }

    private void FillKeypadGrid()
    {
        for (var i = 1; i <= 9; i++)
        {
            AddKeypadButton(i);
        }

        var clearBtn = new Button()
        {
            Text = "C"
        };

        clearBtn.OnPressed += _ => OnClearButtonPressed?.Invoke();

        _keypadGrid.AddChild(clearBtn);

        AddKeypadButton(0);

        var enterBtn = new Button()
        {
            Text = "D"
        };

        enterBtn.OnPressed += _ => OnEnterButtonPressed?.Invoke();

        _keypadGrid.AddChild(enterBtn);
    }

    private void AddKeypadButton(int i)
    {
        var btn = new Button()
        {
            Text = i.ToString()
        };

        btn.OnPressed += _ => OnKeypadButtonPressed?.Invoke(i);

        _keypadGrid.AddChild(btn);
    }

    private void FillCategories()
    {
        var sorted = _prototype
            .EnumeratePrototypes<PhoneCategoryPrototype>()
            .OrderBy(p => p.Index)
            .ToList();

        foreach (var prototype in sorted)
        {
            if (prototype.HideInPhonebook)
                continue;

            var container = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical
            };

            var body = new CollapsibleBody();

            body.AddChild(container);

            var collapsible = new Collapsible();

            collapsible.AddChild(new CollapsibleHeading
            {
                Title = Loc.GetString(prototype.Text)
            });

            collapsible.AddChild(body);

            _phoneBookContainer.AddChild(collapsible);

            _categoryContainers[prototype.ID] = container;
        }
    }

    public void ClearPhoneBook()
    {
        foreach (var container in _categoryContainers.Values)
        {
            container.DisposeAllChildren();
        }
    }

    public void AddPhoneBookLabel(
        string name,
        string category,
        int phonenumber)
    {
        if (!_categoryContainers.TryGetValue(category, out var container))
            return;

        var btn = new Button()
        {
            Text = Loc.GetString(
                "phonebook-format",
                ("name", name),
                ("phonenumber", phonenumber))
        };

        btn.OnPressed += _ =>
            OnPhoneBookButtonPressed?.Invoke(phonenumber);

        container.AddChild(btn);
    }

    // DIAL NUMBER METHODS

    public string GetDialNumber()
    {
        return _dialNumber.GetMessage() ?? string.Empty;
    }

    public void SetDialNumber(string value)
    {
        _dialNumber.SetMessage(value);
    }
}